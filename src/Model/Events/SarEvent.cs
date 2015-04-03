/*
 * Copyright 2015 Matthew Cosand
 */
using System;
using System.Collections.Generic;

namespace Kcsar.Database.Model.Events
{
  public abstract class SarEvent : ModelObject
  {
    public SarEvent()
    {
      this.Participants = new List<Participant>();
      this.Units = new List<ParticipatingUnit>();
      this.Roster = new List<EventRoster>();
      this.Timeline = new List<EventTimeline>();
      this.Followups = new List<SarEvent>();
    }
    public string Title { get; set; }
    public string County { get; set; }
    public string StateNumber { get; set; }
    public string CountyNumber { get; set; }
    public string MissionType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public string Comments { get; set; }
    public string Location { get; set; }
    public bool ReportCompleted { get; set; }

    public virtual ICollection<SarEvent> Followups { get; set; }
    public virtual SarEvent Previous { get; set; }

    public virtual ICollection<EventTimeline> Timeline { get; set; }
    public virtual ICollection<EventRoster> Roster { get; set; }
    public virtual ICollection<Participant> Participants { get; set; }
    public virtual ICollection<ParticipatingUnit> Units { get; set; }

  //  public virtual MissionDetails Details { get; set; }
  //  public virtual ICollection<SubjectGroup> SubjectGroups { get; set; }
  //  public virtual ICollection<MissionGeography> MissionGeography { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("{0} <b>{1}</b> Start:{2}, Type:{3}, County:{4}, Completed:{5}", this.StateNumber, this.Title, this.StartTime, this.MissionType, this.County, this.StopTime);
    }

    public void UpdateRoster()
    {
      throw new System.NotImplementedException();
    }
  }
}
