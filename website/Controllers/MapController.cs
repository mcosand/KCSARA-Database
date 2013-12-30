namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Web.Mvc;
  using Kcsar.Database.Model;
  using Kcsara.Database.Geo;

  [Flags]
  public enum MapModes
  {
    MissionBrowser = 1,
    MissionDetails = 2,
    MemberHouses = 4
  }

  public class MapDataRequests
  {
    public double CenterLat { get; set; }
    public double CenterLong { get; set; }
    public int Zoom { get; set; }
    public MapModes Modes { get; set; }
    public string Title { get; set; }
    public List<string> Messages { get; private set; }

    public List<Guid> Missions { get; private set; }
    public MapDataRequests()
    {
      Missions = new List<Guid>();
    }
  }

  public class MapController : BaseController
  {
    public MapController(IKcsarContext db) : base(db) { }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="id">mission geography id</param>
    ///// <returns></returns>
    //public ActionResult MissionsNear(Guid id)
    //{
    //    MapDataRequests model = new MapDataRequests { Modes = MapModes.MissionDetails, Zoom = 11 };
    //    using (var ctx = GetContext())
    //    {
    //        var center = (from g in ctx.MissionGeography where g.Id == id select g).First().Geography;
    //        model.CenterLat = center.Lat.Value;
    //        model.CenterLong = center.Long.Value;

    //        model.Missions.AddRange(ctx.MissionsNearGeographyId(id, 2).Select(f => f.Value).AsEnumerable());
    //    }

    //    return BuildView("Related Missions", model);
    //}

    public ActionResult Index(Guid? mission)
    {
      var center = GeographyServices.GetDefaultLocation();

      MapDataRequests model = new MapDataRequests { CenterLat = center.Lat.Value, CenterLong = center.Long.Value, Zoom = 9 };

      if (mission.HasValue)
      {
        model.Modes = MapModes.MissionDetails;
        model.Missions.Add(mission.Value);
      }
      else
      {
        model.Modes |= MapModes.MissionBrowser;
        model.Title = "Mission History";
      }

      if (User.IsInRole("cdb.users"))
      {
        model.Title = "";
        model.Modes |= MapModes.MemberHouses;
        ViewData["units"] = UnitsController.GetUnitSelectList(this.db, null);
      }

      return BuildView("Map", model);
    }

    public ActionResult BuildView(string title, MapDataRequests model)
    {
      ViewData["Title"] = title;
      string path = Request.ApplicationPath;
      if (!path.EndsWith("/")) path += '/';

      ViewData["AppRoot"] = Request.Url.GetLeftPart(UriPartial.Authority) + path;

      return View("Index", model);
    }
  }
}