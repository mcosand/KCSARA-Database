using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
  public class TrackingTrip
  {
    public Guid? MemberId { get; set; }

    [ForeignKey("MemberId")]
    public virtual Member Member { get; set; }

    public string Name { get; set; }
    public DateTime Started { get; set; }

  }
}
