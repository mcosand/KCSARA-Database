using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Kcsar.Database.Model;
using log4net;

namespace Kcsara.Database.Web.Controllers
{
  [Authorize]
  public class TrackController : BaseController
  {
    private readonly ILog log;
    public TrackController(IKcsarContext db, ILog log) : base(db)
    {
      this.log = log;
    }

    [HttpGet]
    public ActionResult Index()
    {
      return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult SelfHost(string id, string tracker, decimal? lat = null, decimal? lon = null)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return Content("No username");
      }

      DateTime now = DateTime.Now;
      if (lat.HasValue && lon.HasValue)
      {
        var info = db.Tracks.Where(f => f.Username == id).OrderByDescending(f => f.StartTime).Select(f => new { Trip = f, Idx = f.Points.Count }).FirstOrDefault();
        var trip = info.Trip;
        if (trip == null)
        {
          trip = new Track
          {
            Id = Guid.NewGuid(),
            Name = "Track for " + id,
            StartTime = now,
            Username = id,
            TripId = id + "-" + now.ToString("u")
          };
          db.Tracks.Add(trip);
        }

        var point = new TrackPoint
        {
          Id = Guid.NewGuid(),
          Index = info.Idx,
          Time = DateTime.Now,
          Point = DbGeography.PointFromText(string.Format("POINT({0} {1})", lon.Value, lat.Value), DbGeography.DefaultCoordinateSystemId)
        };

        trip.Points.Add(point);
      }
      else if (tracker == "start" )
      {
        DateTime startDate = DateTime.Now;
        var trip = new Track
        {
          Id = Guid.NewGuid(),
          Name = "Track for " + id,
          StartTime = startDate,
          Username = id,
          TripId = id + "-" + startDate.ToString("u")
        };
        db.Tracks.Add(trip);
      }

      db.SaveChanges();

      return Content("GotIt");
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult BTraced()
    {
      XDocument document = null;
      using (var reader = new System.IO.StreamReader(Request.GetBufferedInputStream()))
      {
        document = XDocument.Parse(reader.ReadToEnd());
      }
   //   log.Debug(document.ToString());

      var user = document.Root.Element("username").Value;
      var timeOffset = long.Parse(document.Root.Element("timeOffset").Value);

      var travel = document.Root.ElementOrDefault("travel");
      if (travel == null)
      {
        this.log.ErrorFormat("Track error for post {0}", document);
        throw new InvalidOperationException("bad request");
      }


      var tripId = int.Parse(travel.Element("id").Value);
      var tripKey = string.Format("{0}-{1}", document.Root.Element("devId"), tripId);
      var description = travel.Element("description").Value;

      var dbTrack = db.Tracks.FirstOrDefault(f => f.TripId == tripKey);
      if (dbTrack == null)
      {
        dbTrack = new Track
        {
          Id = Guid.NewGuid(),
          Name = description,
          StartTime = DateTime.Now,
          Username = user,
          TripId = tripKey
        };
        db.Tracks.Add(dbTrack);
      }

      List<int> pointIds = new List<int>();
      foreach (var point in travel.Elements("point"))
      {
        var hAccuracy = double.Parse(point.Element("haccu").Value);
        var vAccuracy = double.Parse(point.Element("vaccu").Value);
        var geog = DbGeography.PointFromText(string.Format("POINT({0} {1})", point.Element("lon").Value, point.Element("lat").Value), DbGeography.DefaultCoordinateSystemId);
        var bat = double.Parse(point.Element("bat").Value);
        var altitude = double.Parse(point.Element("altitude").Value);
        var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(point.Element("date").Value) - timeOffset).ToLocalTime();
        var pointIndex = int.Parse(point.Element("id").Value);

        var dbPoint = new TrackPoint
        {
          Id = Guid.NewGuid(),
          Track = dbTrack,
          Point = geog,
          Battery = bat,
          HAccuracy = hAccuracy,
          VAccuracy = vAccuracy,
          Altitude = altitude,
          Time = time,
          Index = pointIndex
        };
        if (time < dbTrack.StartTime) {  dbTrack.StartTime = time; }
        pointIds.Add(dbPoint.Index);
        dbTrack.Points.Add(dbPoint);
      }


      db.SaveChanges();

      bool giveUrl = travel.ElementOrDefault("tripUrl") != null && travel.ElementOrDefault("tripUrl").Value == "1";


        //XDocument doc = XDocument.Parse(input);
        //long id = doc.Root.Elements().Where(f => f)
        return Json(new { id = 0, tripid = tripId, points = pointIds.ToArray(), valid = true });
    }
  }
}