using System;

namespace Kcsara.Database.Web.Model.Training
{

  public class TrainingStatus
  {
    public DateTimeOffset? Expires { get; set; }
    public ExpirationFlags Status { get; set; }
    public DateTimeOffset? Completed { get; set; }
    public NameIdPair Course { get; set; }
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
}
