using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using M = Kcsar.Database.Model;

namespace Kcsara.Database.Web.api.Models
{
  public class MissionResponseStatus
  {
    public Guid MissionId { get; set; }
    public string Number { get; set; }
    public string Title { get; set; }
    public DateTime Started { get; set; }

    public bool ActiveResponse { get; set; }
    public DateTime? NextStart { get; set; }
    public DateTime? StopStaging { get; set; }

    public bool ShouldStage
    {
      get
      {
        return this.ActiveResponse && (this.StopStaging == null || this.StopStaging > DateTime.Now);
      }
    }

    public bool ShouldCall
    {
      get
      {
        // The only time a member shouldn't call is if command has said "stop staging" and no future start time is set.
        return this.ActiveResponse && !(this.StopStaging.HasValue && this.StopStaging > this.NextStart && this.StopStaging < DateTime.Now);
      }
    }

    public static readonly Expression<Func<M.Mission, MissionResponseStatus>> FromDatabase =
      m => new MissionResponseStatus
      {
        MissionId = m.Id,
        Number = m.StateNumber,
        Title = m.Title,
        Started = m.StartTime,
        ActiveResponse = m.ResponseStatus != null,
        NextStart = m.ResponseStatus != null ? m.ResponseStatus.CallForPeriod : (DateTime?)null,
        StopStaging = m.ResponseStatus != null ? m.ResponseStatus.StopStaging : null
      };
  }
}