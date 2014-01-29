/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  using System;
  using System.Linq;
  using M = Kcsar.Database.Model;

  public class MissionResponseStatus
  {
    public Mission Mission { get; set; }

    public bool ActiveResponse { get; set; }
    public DateTime? NextStart { get; set; }
    public DateTime? StopStaging { get; set; }
    public RespondingUnit[] ActiveUnits { get; set; }

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

    public virtual MissionResponseStatus LoadData(M.Mission m)
    {
      this.Mission = Mission.FromDatabase(m);
      this.ActiveResponse = m.ResponseStatus != null;
      this.NextStart = m.ResponseStatus != null ? m.ResponseStatus.CallForPeriod : (DateTime?)null;
      this.StopStaging = m.ResponseStatus != null ? m.ResponseStatus.StopStaging : null;
      this.ActiveUnits = m.RespondingUnits.Where(f => f.IsActive).AsEnumerable().Select(f => RespondingUnit.FromDatabase(f)).ToArray();
      return this;
    }

    public static MissionResponseStatus FromData(M.Mission m)
    {
      return (MissionResponseStatus)new MissionResponseStatus().LoadData(m);
    }
  }
}