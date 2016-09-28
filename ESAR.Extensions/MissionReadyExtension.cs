namespace ESAR.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Kcsar.Database.Model;
  using Sar.Database.Api.Extensions;

  public class MissionReadyExtension : IMissionReadyPlugin
  {
    private readonly SarUnit unit;

    public MissionReadyExtension(SarUnit unit)
    {
      this.unit = unit;
    }

    public IEnumerable<string> GetColumnsAfter(MissionReadyColumns column, Member member)
    {
      if (column == MissionReadyColumns.WorkerType)
      {
        var courses = member.ComputedAwards.Where(f =>
          f.Course.Unit != null && f.Course.Unit.Id == this.unit.Id 
          && (f.Expiry == null || f.Expiry > DateTime.Now))
          .Select(f => f.Course.DisplayName)
          .ToArray();
        
        var ranks = new[] { "OL", "FL", "TL" };
        foreach (var rank in ranks)
        {
          if (courses.Any(f => f == this.unit.DisplayName + " " + rank))
          {
            return new [] { rank };
          }
        }
        
        if (member.WacLevel == WacLevel.Field 
          || (member.WacLevel == WacLevel.Novice && member.Memberships.Any(
            f => f.Status.StatusName == "Active"
              && f.Unit.Id == unit.Id
              && (f.EndTime == null || f.EndTime > DateTime.Now))))
        {
          return new[] { "TM" };
        }

        return new[] { string.Empty };
      }
      else
      {
        return new string[0];
      }
    }

    public IEnumerable<string> GetHeadersAfter(MissionReadyColumns column)
    {
      if (column == MissionReadyColumns.WorkerType)
      {
        return new[] { "Rank" };
      }
      return new string[0];
    }
  }
}
