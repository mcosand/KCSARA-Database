/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Web.Http;
  using System.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Model;
  using log4net;

  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  [Authorize]
  public class TrackController : BaseApiController
  {
    public TrackController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    /// <summary>Client trapped a generic unhandled error.</summary>
    /// <param name="error">Error details.</param>
    /// <returns>true</returns>
    public object GetCurrent()
    {
      DateTime now = DateTime.Now;
      DateTime before = now.AddHours(-4);
      var recent = db.Tracks
        .GroupBy(f => f.Username)
        .Select(f => new
        {
          Username = f.Key,
          Point = f.SelectMany(g => g.Points).OrderByDescending(g => g.Time).FirstOrDefault()
        })
        .Where(f => f.Point.Time > before)
        .Select(f => new
        {
          Username = f.Username,
          Time = f.Point.Time,
          Lat = f.Point.Point.Latitude,
          Lon = f.Point.Point.Longitude
        });


      return recent.ToList();
    }
  }
}
