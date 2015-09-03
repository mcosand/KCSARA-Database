using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
  public class Track
  {
    public Track()
    {
      this.Points = new List<TrackPoint>();
    }
    public Guid Id { get; set; }
    public string Username { get; set; }

    public string Name { get; set; }

    public string TripId { get; set; }

    public DateTime StartTime { get; set; }

    public virtual ICollection<TrackPoint> Points { get; set; }
  }

  public class TrackPoint
  {
    public Guid Id { get; set; }

    public Guid TrackId { get; set; }

    public DateTime Time { get; set; }

    public DbGeography Point { get; set; }

    [ForeignKey("TrackId")]
    public virtual Track Track { get; set; }
    public int Index { get; set; }
    public double? HAccuracy { get; set; }
    public double? VAccuracy { get; set; }

    public double? Altitude { get; set; }

    public double? Battery { get; set; }
  }
}
