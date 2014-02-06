using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;

namespace Kcsara.Database.Web.api.Models
{
  public class GeoLocation
  {
    public double accuracy { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }

    public static GeoLocation FromGrography(DbGeography geo)
    {
      if (geo == null) return null;

      return new GeoLocation
      {
        latitude = geo.Latitude.Value,
        longitude = geo.Longitude.Value
      };
    }
  }
}
