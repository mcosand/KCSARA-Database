using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
  public class Location
  {
    public string Type { get; set; }
    public GeoLocation Coords { get; set; }
  }
}