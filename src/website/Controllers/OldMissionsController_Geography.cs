/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Data.SqlTypes;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Web;
  using System.Web.Mvc;
  using System.Xml;
  using System.Xml.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Geo;
  using Kcsara.Database.Web.Model;
  using Microsoft.SqlServer.Types;

  public partial class MissionsController
  {
    [Authorize(Roles = "cdb.missioneditors")]
    [HttpGet]
    public ActionResult UploadGpx(Guid id)
    {
      return View(this.db.Missions.First(f => f.Id == id));
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [HttpPost]
    public ActionResult UploadGpx(Guid id, FormCollection fields)
    {
      Mission_Old mission = this.db.Missions.Single(f => f.Id == id);
      string kind = null;
      string desc = null;

      if (string.IsNullOrWhiteSpace(fields["description"]))
      {
        ModelState.AddModelError("description", "description is required");
      }
      else
      {
        desc = fields["description"];
        ModelState.SetModelValue("description", new ValueProviderResult(desc, desc, CultureInfo.CurrentUICulture));
      }

      if (string.IsNullOrWhiteSpace(fields["kind"]) || fields["kind"] != "teamtrk")
      {
        ModelState.AddModelError("kind", "only 'Team Track' is supported");
      }
      else
      {
        kind = fields["kind"];
        ModelState.SetModelValue("kind", new ValueProviderResult(kind, kind, CultureInfo.CurrentUICulture));
      }

      if (Request.Files.Count == 0)
      {
        ModelState.AddModelError("file", "No file uploaded");
      }
      else if (Request.Files.Count > 1)
      {
        return new ContentResult { Content = "Invalid Request: Too many files" };
      }

      HttpPostedFileBase hpf = null;
      if (ModelState.IsValid)
      {
        hpf = Request.Files[0] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
        {
          ModelState.AddModelError("file", "No file uploaded");
        }
      }

      if (ModelState.IsValid)
      {
        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(hpf.InputStream);

          XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
          ns.AddNamespace("g", "http://www.topografix.com/GPX/1/1");

          XmlNodeList points = null;
          XmlNodeList tracks = doc.SelectNodes("//g:trk", ns);
          if (tracks.Count < 1)
          {
            ModelState.AddModelError("file", "At least one track required");
          }
          else
          {
            foreach (var track in tracks.Cast<XmlNode>())
            {
              string localDesc = desc;
              var name = track.SelectSingleNode("g:name", ns);
              if (name != null && !string.IsNullOrWhiteSpace(name.InnerText))
              {
                localDesc = name.InnerText;
              }

              points = track.SelectNodes("g:trkseg/g:trkpt", ns);
              if (points != null && points.Count > 2)
              {
                DateTime? minDate;
                //if (!TryExtractGpxGeography(ns, points, out geo, out minDate, 1, points.Count))
                //{
                //    geo = ExtractGeographyExcludeErrors(ns, points, out minDate);
                //}
                SqlGeography geo = ExtractGeography(ns, points, out minDate);
                if (geo == null) continue;

                MissionGeography geog = new MissionGeography
                {
                  Mission = mission,
                  Description = localDesc,
                  Kind = kind,
                  Geography = geo,
                  Time = minDate
                };
                this.db.MissionGeography.Add(geog);
              }
            }
          }
        }
        catch (XmlException)
        {
          ModelState.AddModelError("file", "Invalid GPX file");
        }
      }
      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        return ClosePopup();
      }
      return View(mission);
    }

    private SqlGeography ExtractGeography(XmlNamespaceManager ns, XmlNodeList points, out DateTime? minDate)
    {
      minDate = null;
      SqlGeography result = null;

      if (points.Count < 2) return null;

      string goodString = GetValidCoordinateString(points.Cast<XmlElement>().Select(f => GetCoordinate(ns, f)).ToList());

      // Check for multiple points in the line
      if (goodString.IndexOf(',') >= 0)
      {
        result = SqlGeography.STLineFromText(new SqlChars("LINESTRING(" + goodString + ")"), GeographyServices.SRID);
      }

      return result;
    }


    private string GetValidCoordinateString(IList<string> points)
    {
      int count = points.Count;
      SqlGeography geography = null;

      if (count < 2)
        return null;


      if (count < 5)
      {
        StringBuilder builder = new StringBuilder();
        builder.Append(points[0]);

        for (int i = 1; i < points.Count; i++)
        {
          try
          {
            string newPoint = ", " + points[i];
            SqlGeography.STLineFromText(new SqlChars("LINESTRING(" + builder.ToString() + newPoint + ")"), GeographyServices.SRID);
            builder.Append(newPoint);
          }
          catch (ArgumentException)
          {
          }
          catch (FormatException)
          {
          }
          return builder.ToString();
        }
      }
      else
      {
        string coordinateString = string.Join(", ", points);
        try
        {
          geography = SqlGeography.STLineFromText(new SqlChars("LINESTRING(" + coordinateString + ")"), GeographyServices.SRID);
          return coordinateString;
        }
        catch (ArgumentException)
        {
        }
        catch (FormatException)
        {
        }

        string left = GetValidCoordinateString(points.Take(count / 2).ToList());
        string right = GetValidCoordinateString(points.Skip(count / 2).ToList());
        int comma = -1;
        string test = null;
        while (geography == null && (comma = right.IndexOf(',')) >= 0)
        {
          test = left + ", " + right;
          try
          {
            geography = SqlGeography.STLineFromText(new SqlChars("LINESTRING(" + test + ")"), GeographyServices.SRID);
          }
          catch (ArgumentException)
          {
            right = right.Substring(comma + 1).TrimStart();
          }
          catch (FormatException)
          {
            right = right.Substring(comma + 1).TrimStart();
          }
        }

        if (left.IndexOf(',') < 0 && right.IndexOf(',') < 0)
        {
          if (string.Join(" ", left.Split(' ').Take(2)) == string.Join(" ", right.Split(' ').Take(2)))
          {
            return left;
          }
        }

        return left + ", " + right;
      }

      return "";
    }

    /// <summary>
    /// Convert a GPX track point to a well-known-text string
    /// </summary>
    /// <param name="ns"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private string GetCoordinate(XmlNamespaceManager ns, XmlNode node)
    {
      double? date = GetDateValueFromTrackpoint(node, ns);
      double lat = double.Parse(node.Attributes["lat"].Value);
      double lon = double.Parse(node.Attributes["lon"].Value);
      double? elev = (node.SelectSingleNode("g:ele", ns) == null) ? (double?)null : double.Parse(node.SelectSingleNode("g:ele", ns).InnerText);

      return string.Format("{0} {1} {2} {3}", lon, lat, elev, date);
    }

    private double? GetDateValueFromTrackpoint(XmlNode trackpoint, XmlNamespaceManager ns)
    {
      double? retVal = null;
      if (trackpoint.SelectSingleNode("g:time", ns) != null)
      {
        retVal = DateTime.Parse(trackpoint.SelectSingleNode("g:time", ns).InnerText).ToOADate();
      }
      return retVal;
    }

    [HttpPost]
    public ActionResult DeleteWaypoint(Guid id)
    {
      if (!User.IsInRole("cdb.missioneditors"))
      {
        return GetLoginError();
      }

      var waypoint = (from w in this.db.MissionGeography where w.Id == id select w).First();
      this.db.MissionGeography.Remove(waypoint);
      this.db.SaveChanges();

      return Data(new SubmitResult<bool> { Result = true, Errors = new SubmitError[0] });
    }

    [HttpPost]
    public ActionResult SubmitWaypoint(WaypointView wpt)
    {
      List<SubmitError> errors = new List<SubmitError>();
      Guid result = Guid.Empty;

      if (!User.IsInRole("cdb.missioneditors"))
      {
        return GetLoginError();
      }

      MissionGeography geog = null;

      geog = (from g in this.db.MissionGeography where g.Id == wpt.Id select g).FirstOrDefault();
      if (geog == null)
      {
        geog = new MissionGeography { Mission = (from m in this.db.Missions where m.Id == wpt.EventId select m).First() };
        this.db.MissionGeography.Add(geog);
      }

      //try
      //{
        if (geog.Kind != wpt.Kind) geog.Kind = wpt.Kind;
        if (geog.InstanceId != wpt.InstanceId) geog.InstanceId = wpt.InstanceId;

        SqlGeography defaultCoord = GeographyServices.GetDefaultLocation();
        wpt.Lat = Math.Abs(wpt.Lat) * Math.Sign(defaultCoord.Lat.Value);
        wpt.Long = Math.Abs(wpt.Long) * Math.Sign(defaultCoord.Long.Value);

        SqlGeography geography = wpt.AsSqlGeography();
        if (string.Format("{0}", geog.Geography) != string.Format("{0}", geography)) geog.Geography = geography;
        if (geog.Description != wpt.Description) geog.Description = wpt.Description;
        if (geog.Time != wpt.Time) geog.Time = wpt.Time;

        if (errors.Count == 0)
        {
          this.db.SaveChanges();
        }

      //}
      //catch (RuleViolationsException ex)
      //{
      //  //this.CollectRuleViolations(ex, fields);
      //  foreach (RuleViolation v in ex.Errors)
      //  {
      //    errors.Add(new SubmitError { Error = v.ErrorMessage, Property = v.PropertyName, Id = new[] { v.EntityKey } });
      //  }
      //}

      return Data(new SubmitResult<GeographyView>
      {
        Errors = errors.ToArray(),
        Result = (errors.Count > 0) ?
            (GeographyView)null :
            GeographyView.BuildGeographyView(geog)
        //new WaypointView
        //{
        //    Id = newView.Id,
        //    MissionId = newView.Mission.Id,
        //    Kind = newView.Kind,
        //    Desc = newView.Description,
        //    Lat = GeographyServices.FormatCoordinate(newView.Geography.Lat.Value, this.UserSettings.CoordinateDisplay),
        //    Long = GeographyServices.FormatCoordinate(newView.Geography.Long.Value, this.UserSettings.CoordinateDisplay),
        //    Instance = newView.InstanceId,
        //    Time = newView.Geography.M.IsNull ? (DateTime?)null : DateTime.FromOADate(newView.Geography.M.Value)
        //}
      });
    }

    [HttpPost]
    public JsonDataContractResult GetGeographies(DateTime? start, DateTime? stop, bool? overview)
    {
      MapDataView model = new MapDataView();
      List<string> messages = new List<string>();
      var query = (from g in this.db.MissionGeography.Include("Mission") select g);
      if (start.HasValue)
      {
        query = query.Where(f => f.Mission.StartTime >= start.Value);
      }
      if (stop.HasValue)
      {
        query = query.Where(f => f.Mission.StopTime < stop.Value);
      }

      Guid lastMission = Guid.Empty;
      int excluded = 0;

      Dictionary<Mission_Old, GeographyView> views = new Dictionary<Mission_Old, GeographyView>();

      foreach (MissionGeography geo in query.ToList().OrderBy(f => f.Mission.StartTime).ThenBy(f => f.Mission.StateNumber).ThenBy(f => f.Mission.Id).ThenBy(f => f.Geography.STDimension()).ThenByDescending(f => f.Kind))
      {
        if (geo.Mission.Id == lastMission)
        {
          continue;
        }

        if (!User.IsInRole("cdb.users") && (geo.Mission.MissionType.ToLowerInvariant().Contains("urban") || geo.Mission.MissionType.ToLowerInvariant().Contains("evidence")))
        {
          excluded++;
          lastMission = geo.Mission.Id;
          continue;
        }

        if ((overview ?? true) && !(geo.Kind == "found" || geo.Kind == "base" || geo.Kind == "cluLkp"))
        {
          continue;
        }

        if (geo.Geography.STDimension() == 1)
        {
          geo.Geography = geo.Geography.STPointN(1);
        }

        GeographyView view = GeographyView.BuildGeographyView(geo);

        view.Description = string.Format("{0:yyyy-MM-dd}, #{1}<br/>{2}<br/>{3}",
            geo.Mission.StartTime.Date,
            geo.Mission.StateNumber,
            geo.Mission.Title,
            User.IsInRole("cdb.users") ? "<a target=\"_blank\" href=\"" + Url.Action("geography", new { id = geo.Mission.Id }) + "\">View Details</a>" : ""
            );
        view.EventId = geo.Mission.Id;

        model.Items.Add(view);

        lastMission = geo.Mission.Id;
      }

      if (excluded > 0)
      {
        messages.Add(string.Format("{0} evidence and/or urban searches not shown to anonymous users.", excluded));
      }

      if (messages.Count > 0)
      {
        model.Messages = messages.ToArray();
      }

      return new JsonDataContractResult(model);
    }

    [HttpPost]
    public DataActionResult GetGeography(Guid id, bool? overview)
    {
      if (!User.IsInRole("cdb.users")) return GetLoginError();

      MapDataView model = new MapDataView();
      var query = (from g in this.db.MissionGeography where g.Mission.Id == id select g);
      if (overview.HasValue && overview.Value)
      {
        query = query.Where(f => f.Kind == "found" || f.Kind == "base" || f.Kind == "cluLkp").OrderByDescending(f => f.Kind).Take(1);
      }

      model.Items.AddRange(query.AsEnumerable().Select(f => GeographyView.BuildGeographyView(f)));
      return Data(model);
    }

    [Authorize]
    public FileContentResult MissionsKML(DateTime? begin, DateTime? end)
    {
      List<GeographyView> items = ((MapDataView)GetGeographies(begin, end, false).Data).Items;
      items.Reverse();

      Dictionary<Guid, Tuple<int, int>> detailCounts = (from mg in this.db.MissionGeography group mg by mg.Mission.Id into g select new { Id = g.Key, Points = g.Count(f => !f.Kind.EndsWith("trk")), Tracks = g.Count(f => f.Kind.EndsWith("trk")) }).ToDictionary(f => f.Id, f => new Tuple<int, int>(f.Points, f.Tracks));

      KmlBuilder kml = (new KmlBuilder { Name = "Mission Data", Description = "Mission data from KCSARA ()" }).AddIconStyles(this.AbsoluteUrl(Url.Content("~/content/images/maps")));

      Guid overview = kml.CreateFolder("Overview", null);

      foreach (var item in items)
      {
        string[] lines = item.Description.Split(new[] { "<br/>" }, StringSplitOptions.RemoveEmptyEntries);
        item.Description = lines[1] + "<br/>" + lines[0].Split(',')[0];

        string dem = lines[0].Split('#')[1];
        kml.AddItem(item, dem, item.Kind, null, overview);
        int points = detailCounts[(Guid)item.EventId].Item1;
        int tracks = detailCounts[(Guid)item.EventId].Item2;
        if (points > 1 || tracks > 0)
        {
          kml.AddLink(string.Format("{0} {1} ({2})", dem, lines[1], points + tracks), null, AbsoluteUrl(Url.Action("MissionKML", new { id = item.EventId, _auth = "basic" })), false, null);
        }
      }

      return new FileContentResult(Encoding.UTF8.GetBytes(kml.ToString()), "application/vnd.google-earth.kml+xml") { FileDownloadName = "kcsara-missions.kml" };
    }

    [Authorize]
    public FileContentResult MissionKML(Guid id)
    {
      MapDataView data = (MapDataView)GetGeography(id, false).Data;

      Mission_Old m = (from mission in this.db.Missions where mission.Id == id select mission).FirstOrDefault();

      XNamespace ns = "http://earth.google.com/kml/2.1";
      KmlBuilder kml = (new KmlBuilder { Name = string.Format("{0}: {1}", m.StateNumber, m.Title), Description = "" }).AddIconStyles(this.AbsoluteUrl(Url.Content("~/content/images/maps")));


      string[] colors = "ff4500,ff42c3,904cff".Split(',');
      Guid teamFolder = Guid.Empty;

      foreach (var item in data.Items)
      {
        Guid? folder = null;
        if (item.Kind == "team")
        {
          if (teamFolder == Guid.Empty)
          {
            teamFolder = kml.CreateFolder("Team Positions", null);
          }
          folder = teamFolder;
        }

        string name = string.IsNullOrWhiteSpace(item.Description) ? item.Kind : item.Description;

        kml.AddItem(item, name, item.Kind, null, folder);


        //var itemNode = new XElement(ns + "Placemark");
        //itemNode.Add(new XElement(ns + "name", item.Description));
        //itemNode.Add(new XElement(ns + "description", item.Description));

        //if (item is RouteView)
        //{
        //    itemNode.Add(new XElement(ns + "styleUrl", "#_" + colors[i++ % colors.Length]));
        //    var lineNode = new XElement(ns + "LineString");
        //    lineNode.Add(new XElement(ns + "extrude", 1));
        //    lineNode.Add(new XElement(ns + "tessellate", 1));
        //    lineNode.Add(new XElement(ns + "altitudeMode", "clampToGround"));
        //    //lineNode.Add(new XElement(ns + "color", colors[i++ % colors.Length]+"ff"));
        //    lineNode.Add(new XElement(ns + "coordinates", string.Join("\n", ((RouteView)item).Points.Select(f => string.Format("{0},{1},10", f.Long, f.Lat)).ToArray())));
        //    itemNode.Add(lineNode);
        //}
        //else if (item is WaypointView)
        //{
        //    WaypointView wpt = (WaypointView)item;
        //    itemNode.Add(new XElement(ns + "Point", new XElement(ns + "coordinates", string.Format("{0},{1}", wpt.Long, wpt.Lat))));
        //    if (item.Kind == "team")
        //    {
        //        itemNode.Add(new XElement(ns + "visibility", 0));
        //    }
        //}
        //docNode.Add(itemNode);
      }



      return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(kml.ToString()), "application/vnd.google-earth.kml+xml") { FileDownloadName = m.StateNumber + ".kml" };

    }

    [HttpPost]
    public ActionResult GetGeographyInstances(Guid missionId, string kind)
    {
      if (!User.IsInRole("cdb.missioneditors"))
      {
        return GetLoginError();
      }

      Dictionary<Guid, string> result = null;
      if (kind == "found" || kind == "extract" || kind == "cluLkp")
      {
        result = (from g in this.db.SubjectGroups where g.Mission.Id == missionId select g).ToDictionary(f => f.Id, f => "Group " + (f.Number - 1));
      }
      return Data(new SubmitResult<Dictionary<Guid, string>> { Result = result });
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult Geography(Guid id)
    {
      MapDataView model = new MapDataView();
      Mission_Old m = (from g in this.db.Missions where g.Id == id select g).First();
      ViewData["Title"] = string.Format("Mission Geography{0}", " :: " + m.StateNumber + " " + m.Title);
      ViewData["mission"] = m;

      model.Id = id;
      ViewData["coordDisplay"] = CoordinateDisplay.DecimalMinutes;
      ViewData["missionId"] = id;
      return View(model);
    }

    private double ParseCoordinate(string coord)
    {
      if (coord != null)
      {
        double d;
        Match m = Regex.Match(coord.Trim(), @"^\-?(\d{2,3}\.\d+)$");
        if (m.Success && double.TryParse(m.Groups[1].Value, out d))
        {
          return d;
        }

        m = Regex.Match(coord.Trim(), @"^\-?(\d{2,3}) (\d{1,2}\.\d+)$");
        if (m.Success)
        {
          return int.Parse(m.Groups[1].Value) + double.Parse(m.Groups[2].Value) / 60.0;
        }

        m = Regex.Match(coord.Trim(), @"^\-?(\d{2,3}) (\d{1,2}) (\d{1,2}\.\d+)$");
        if (m.Success)
        {
          return int.Parse(m.Groups[1].Value) + (double.Parse(m.Groups[2].Value) + double.Parse(m.Groups[3].Value) / 60.0) / 60.0;
        }
      }
      throw new FormatException("Coordinates must be in form 'dd.dddd', 'dd mm.mmmm', or 'dd mm ss.sss'");
    }
  }
}
