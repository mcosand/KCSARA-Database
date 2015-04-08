/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("SarEvents")]
  public abstract class SarEventRow : ModelObjectRow
  {
    public SarEventRow()
    {
      this.Participants = new List<ParticipantRow>();
      this.Units = new List<ParticipatingUnitRow>();
      this.Roster = new List<EventRosterRow>();
      this.Timeline = new List<EventTimelineRow>();
      this.Followups = new List<SarEventRow>();
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

    [ForeignKey("PreviousId")]
    public virtual SarEventRow Previous { get; set; }
    public Guid? PreviousId { get; set; }

    public virtual ICollection<SarEventRow> Followups { get; set; }
    public virtual ICollection<EventTimelineRow> Timeline { get; set; }
    public virtual ICollection<EventRosterRow> Roster { get; set; }
    public virtual ICollection<ParticipantRow> Participants { get; set; }
    public virtual ICollection<ParticipatingUnitRow> Units { get; set; }

    public virtual EventDetailRow Details { get; set; }
    public virtual ICollection<SubjectRow> Subjects { get; set; }
    public virtual ICollection<SubjectGroupRow> SubjectGroups { get; set; }
    public virtual ICollection<EventGeographyRow> MissionGeography { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("{0} <b>{1}</b> Start:{2}, Type:{3}, County:{4}, Completed:{5}", this.StateNumber, this.Title, this.StartTime, this.MissionType, this.County, this.StopTime);
    }

    public void UpdateRoster()
    {
      throw new System.NotImplementedException();
    }
  }

  public class MissionRow : SarEventRow
  {
  }

  public class TrainingRow : SarEventRow
  {
    public virtual ICollection<TrainingCourseRow> OfferedCourses { get; set; }
  }
}
