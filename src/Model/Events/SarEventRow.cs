/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Model.Events
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Text.RegularExpressions;

  [Table("Events")]
  public class SarEventRow : ModelObject
  {
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

    public virtual ICollection<SarEventRow> Followups { get; set; }
    public virtual ICollection<EventLogRow> Log { get; set; }

    [Column("Previous_Id")]
    public Guid? PreviousId { get; set; }

    [ForeignKey("PreviousId")]
    public virtual SarEventRow Previous { get; set; }

    //   public virtual ICollection<MissionLog> Log { get; set; }
    public virtual ICollection<EventParticipantRow> Participants { get; set; }
    public virtual ICollection<EventUnitRow> Units { get; set; }

    //  public virtual MissionDetails Details { get; set; }
    //  public virtual ICollection<SubjectGroup> SubjectGroups { get; set; }
    //  public virtual ICollection<MissionGeography> MissionGeography { get; set; }

    public SarEventRow()
      : base()
    {
      this.StartTime = DateTime.Now.Date;
      this.County = "King";
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", this.StateNumber, this.Title);
    }

    bool stopDirty = false;
    bool startDirty = false;

    public override string GetReportHtml()
    {
      return string.Format("{0} <b>{1}</b> Start:{2}, Type:{3}, County:{4}, Completed:{5}", this.StateNumber, this.Title, this.StartTime, this.MissionType, this.County, this.StopTime);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (string.IsNullOrEmpty(this.Title))
      {
        yield return new ValidationResult("Required", new[] { "Title" });
      }

      if (string.IsNullOrEmpty(this.Location))
      {
        yield return new ValidationResult("Required", new[] { "Location" });
      }

      if (string.IsNullOrEmpty(this.County))
      {
        yield return new ValidationResult("Required", new[] { "County" });
      }
      else if (!Utils.CountyNames.Contains(this.County, new Utils.CaseInsensitiveComparer()))
      {
        yield return new ValidationResult("Not a county in Washington", new[] { "County" });
      }


      if (this.StopTime.HasValue)
      {
        string field = (startDirty && !stopDirty) ? "StartTime" : "StopTime";
        string badValue = ((startDirty && !stopDirty) ? this.StartTime : this.StopTime.Value).ToString("MM/dd/yy HH:mm");

        if (this.StopTime <= this.StartTime)
        {
          yield return new ValidationResult("Start must be before Stop", new[] { field });
        }
      }

      if (!string.IsNullOrEmpty(this.StateNumber) && !Regex.IsMatch(this.StateNumber, @"^\d{2}\-\d{4}$") && !Regex.IsMatch(this.StateNumber, @"^\d{2}\-ES\-\d{3}$", RegexOptions.IgnoreCase))
      {
        yield return new ValidationResult("Must be in form 00-0000 or 00-ES-000", new[] { "StateNumber" });
      }

      if (!string.IsNullOrEmpty(this.CountyNumber) && this.County.Equals("King", StringComparison.OrdinalIgnoreCase) && !Regex.IsMatch(this.CountyNumber, @"^\d{2}\-\d{6}$"))
      {
        yield return new ValidationResult("Must be in form 00-000000", new[] { "CountyNumber" });
      }

      if (this.Previous != null && this.Previous.Id == this.Id)
      {
        yield return new ValidationResult("Can't link a mission to itself", new[] { "Previous" });
      }
    }
  }
  public class MissionRow : SarEventRow
  {
    public static MissionRow FromOldModel(Mission_Old m)
    {
      var newMission = new MissionRow
      {
        Id = m.Id,
        ChangedBy = m.ChangedBy,
        CountyNumber = m.CountyNumber,
        County = m.County,
        Comments = m.Comments,
        Location = m.Location,
        MissionType = m.MissionType,
        PreviousId = m.Previous == null ? (Guid?)null : m.Previous.Id,
        ReportCompleted = m.ReportCompleted,
        StartTime = m.StartTime,
        StopTime = m.StopTime,
        StateNumber = m.StateNumber,
        Title = m.Title
      };
      return newMission;
    }
  }

  public class TrainingRow : SarEventRow
  {
    public static TrainingRow FromOldModel(Training_Old t)
    {
      var newTraining = new TrainingRow
      {
        Id = t.Id,
        ChangedBy = t.ChangedBy,
        County = t.County,
        Comments = t.Comments,
        Location = t.Location,
        StartTime = t.StartTime,
        StopTime = t.StopTime,
        StateNumber = t.StateNumber,
        Title = t.Title
      };
      return newTraining;
    }
  }
}
