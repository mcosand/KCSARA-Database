/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Data.Entity.Spatial;
  using System.Linq;
  using System.Web.Mvc;
  using System.Xml.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Geo;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.Model;

  public class HomeController : BaseController
  {
    private readonly IReportsService reports;
    public HomeController(IKcsarContext db, IReportsService reports, IAppSettings settings)
      : base(db, settings)
    {
      System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<MeshNodeEntities>());
      this.reports = reports;
    }

    public ActionResult Index()
    {
      ViewData["Title"] = Strings.DatabaseName;

      return View();
    }

    public ActionResult About()
    {
      ViewData["Title"] = "About Page";

      return View();
    }

    public ActionResult Error(string message)
    {
      if (!string.IsNullOrWhiteSpace(message))
      {
        ViewBag.Message = message;
      }
      return View();
    }

    public ActionResult CountyReport()
    {
      if (!Permissions.IsUserOrLocal(Request)) return this.CreateLoginRedirect();
      var result = this.reports.GetMembershipReport();

      return this.File(result, "application/vnd.ms-excel", string.Format("{0}-Report-{1:yyMMdd}.xls", this.settings.GroupName, DateTime.Now));
    }


    private static DateTime UnixEpoch = new DateTime(1969, 12, 31, 17, 0, 0); // Jan 1, 1970 UTC in local time

    public ActionResult NodeCheckinData(string id)
    {
      if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-zA-Z\\-0-9]+$"))
      {
        return new ContentResult { ContentType = "text/plain", Content = "Bad id" };
      }
      if (Request.Files.Count == 0) return new ContentResult { Content = "no file", ContentType = "text/plain" };

      var hpf = Request.Files[0] as System.Web.HttpPostedFileBase;
      XDocument doc = XDocument.Load(hpf.InputStream, LoadOptions.PreserveWhitespace);

      using (var mnc = new MeshNodeEntities())
      {
        var locElement = doc.Element("MeshReport").Element("Location").Element("Last");
        string[] loc = locElement.Attribute("Position").Value.Split(',');
        var status = new MeshNodeStatus
        {
          Location = (loc.Length > 1) ? DbGeography.PointFromText(string.Format("POINT({0} {1})", loc[1], loc[0]), GeographyServices.SRID) : null,
          Time = DateTime.Parse(doc.Element("MeshReport").Attribute("time").Value),
          Name = id,
          IPAddr = doc.ElementOrDefault("Modem").ElementOrDefault("IP").SafeValue()
        };

        try
        {
          float uptime;
          if (float.TryParse(doc.ElementOrDefault("Uptime").SafeValue(), out uptime))
          {
            status.Uptime = uptime;
          }

          var volts = doc.ElementOrDefault("Voltages").SafeValue();
          if (!string.IsNullOrWhiteSpace(volts))
          {
            var voltParts = volts.Split(' ');
            status.BatteryVolts = float.Parse(voltParts[0]);
            status.HouseVolts = float.Parse(voltParts[1]);
            status.AlternatorVolts = float.Parse(voltParts[2]);
          }
        }
        catch (Exception)
        {
          // eat
        }

        foreach (var existing in mnc.Checkins.Where(f => f.Name == id))
        {
          mnc.Checkins.Remove(existing);
        }
        mnc.Checkins.Add(status);
        mnc.SaveChanges();

        string track = doc.ElementOrDefault("Location").ElementOrDefault("History").SafeValue();
        if (!string.IsNullOrWhiteSpace(track))
        {
          string[] trackParts = track.Trim().Split('\n');
          DateTime start = UnixEpoch.AddSeconds(int.Parse(trackParts[0].Split(',')[0]));
          DateTime stop = UnixEpoch.AddSeconds(int.Parse(trackParts[trackParts.Length - 1].Split(',')[0]));

          MeshNodeLocation before = mnc.Locations.Where(f => f.Name == id && f.Time < start).OrderByDescending(f => f.Time).FirstOrDefault();
          MeshNodeLocation after = mnc.Locations.Where(f => f.Name == id && f.Time > stop).OrderBy(f => f.Time).FirstOrDefault();

          foreach (var existing in mnc.Locations.Where(f => f.Name == id && f.Time >= start && f.Time <= stop))
          {
            mnc.Locations.Remove(existing);
          }

          foreach (var report in trackParts)
          {
            string[] parts = report.Split(',');
            DateTime time = UnixEpoch.AddSeconds(int.Parse(parts[0]));
            if (string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2])) continue;

            DbGeography location = DbGeography.PointFromText(string.Format("POINT({0} {1})", parts[2], parts[1]), GeographyServices.SRID);

            if ((before == null || location.Distance(before.Location) > 20) && (after == null || location.Distance(after.Location) > 10)) // meters
            {
              MeshNodeLocation newLoc = new MeshNodeLocation
              {
                Name = id,
                Time = time,
                Location = location
              };
              mnc.Locations.Add(newLoc);
              before = newLoc;
            }
          }

          mnc.SaveChanges();
        }
      }

      return new ContentResult { Content = "Thanks", ContentType = "text/plain" };

    }

    [Authorize]
    public ActionResult MeshStatus()
    {
      MeshNodeStatus[] model;
      using (var mnc = new MeshNodeEntities())
      {
        model = mnc.Checkins.ToArray();
      }
      return View(model);
    }

    [Authorize]
    public ActionResult MeshGraphs(string id, string type)
    {
      MeshNodeStatus model;
      using (var mnc = new MeshNodeEntities())
      {
        model = mnc.Checkins.Single(f => f.Name == id);
      }
      ViewData["type"] = type;
      return View(model);
    }

    [Authorize]
    public ActionResult MeshNodeTrack(string id, DateTime? start, DateTime? stop)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return new ContentResult { Content = "Invalid id" };
      }

      start = start ?? DateTime.Today.AddDays(-1);
      stop = stop ?? DateTime.Now;

      GpxDocument gpx = new GpxDocument();
      gpx.StartTrack(id + " track");
      using (var mnc = new MeshNodeEntities())
      {
        foreach (var point in (from p in mnc.Locations where p.Name == id && p.Time >= start && p.Time <= stop select p))
        {
          gpx.AppendToTrack(point.Location.Latitude.Value, point.Location.Longitude.Value, null, point.Time);
        }
      }
      gpx.FinishTrack();

      return new FileContentResult(gpx.ToUtf8(), "text/xml") { FileDownloadName = id + ".gpx" };
    }

    public ActionResult NodeCheckin(string id)
    {
      if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-zA-Z\\-0-9]+$"))
      {
        return new ContentResult { ContentType = "text/plain", Content = "Bad id" };
      }
      if (Request.Files.Count == 0) return new ContentResult { Content = "no file", ContentType = "text/plain" };

      string storepath = Server.MapPath("~/Content/auth/nodes/") + id + ".txt";
      var hpf = Request.Files[0] as System.Web.HttpPostedFileBase;

      using (var fs = new System.IO.FileStream(storepath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read))
      {
        byte[] buffer = new byte[8 * 1024];
        int len;
        while ((len = hpf.InputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
          fs.Write(buffer, 0, len);
        }
      }

      //string logPath = Server.MapPath("~/Content/auth/nodes/") + id + System.DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
      //if (!System.IO.File.Exists(logPath))
      //{
      //    System.IO.File.Copy(storepath, logPath);
      //}

      return new ContentResult { Content = "Thanks", ContentType = "text/plain" };
    }

    public ActionResult NodeGraphCheckin(string id, string window, string graph)
    {
      if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-zA-Z\\-0-9]+$"))
      {
        return new ContentResult { ContentType = "text/plain", Content = "Bad id" };
      }

      if (graph != null && !System.Text.RegularExpressions.Regex.IsMatch(graph, "^[a-zA-Z\\-0-9]+$"))
      {
        return new ContentResult { ContentType = "text/plain", Content = "Bad graph" };
      }

      if (Request.Files.Count == 0) return new ContentResult { Content = "no file", ContentType = "text/plain" };

      string storepath = string.Format("{0}{1}-{2}{3}.gif", Server.MapPath("~/Content/auth/nodes/"), id, graph, window);
      var hpf = Request.Files[0] as System.Web.HttpPostedFileBase;

      using (var fs = new System.IO.FileStream(storepath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read))
      {
        byte[] buffer = new byte[8 * 1024];
        int len;
        while ((len = hpf.InputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
          fs.Write(buffer, 0, len);
        }
      }

      return new ContentResult { Content = "Thanks", ContentType = "text/plain" };
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">External table name</param>
    /// <returns></returns>
    [HttpPost]
    public DataActionResult GetExternalKeys(string id)
    {
      if (!Permissions.IsAdmin) return Data("bad username");

      return Data(this.db.xref_county_id.Where(f => f.ExternalSource == id).ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="kcsaraId"></param>
    /// <param name="externalId"></param>
    /// <param name="id">The external table name</param>
    /// <returns></returns>
    [HttpPost]
    public ContentResult UpdateExternalKey(Guid kcsaraId, int externalId, string id)
    {
      if (!Permissions.IsInRole("cdb.admins")) throw new InvalidOperationException();

      var entry = this.db.xref_county_id.Where(f => f.personId == kcsaraId && f.ExternalSource == id).SingleOrDefault();

      if (entry == null)
      {
        entry = new Kcsar.Database.Model.xref_county_id { personId = kcsaraId, ExternalSource = id };
        this.db.xref_county_id.Add(entry);
      }

      entry.accessMemberID = externalId;
      this.db.SaveChanges();

      return new ContentResult { Content = "OK", ContentType = "text/plain" };
    }
  }
}
