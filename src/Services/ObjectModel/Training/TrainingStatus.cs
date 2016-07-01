using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Model.Training
{

  public class TrainingStatus
  {
    public DateTime? Expires { get; set; }
    public ExpirationFlags Status { get; set; }
    public DateTime? Completed { get; set; }
    public NameIdPair Course { get; set; }

    //public override string ToString()
    //{
    //  return this.Expires.HasValue
    //              ? this.Expires.Value.ToString("yyyy-MM-dd")
    //              : (this.Status == ExpirationFlags.NotNeeded) ? "" : this.Status.ToString();
    //}
  }

  [Flags]
  public enum ExpirationFlags
  {
    Unknown = 0,
    Bad = 1,
    Missing = 3,
    Expired = 5,
    ToBeCompleted = 2,
    NotNeeded = 8,
    Complete = 16,
    NotExpired = 32
  }

  //[Flags]
  //public enum ExpirationFlags
  //{
  //  Unknown = 0,
  //  Okay = 1,
  //  NotNeeded = 5, // 4 + 1
  //  Complete = 9, // 8 + 1
  //  NotExpired = 17, // 16 + 1
  //  Expired = 32,
  //  Missing = 64,
  //}

}
