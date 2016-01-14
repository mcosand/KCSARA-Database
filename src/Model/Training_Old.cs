﻿/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Text.RegularExpressions;

  [Table("Trainings")]
  public class Training_Old : ModelObject, IRosterEvent<Training_Old, TrainingRoster_Old>
  {
    [Required]
    public string Title { get; set; }
    public string County { get; set; }
    public string StateNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public Guid? Previous { get; set; }
    public string Comments { get; set; }
    public virtual ICollection<TrainingRoster_Old> Roster { get; set; }
    public virtual ICollection<TrainingCourse> OfferedCourses { get; set; }
    [Required]
    public string Location { get; set; }

    public virtual ICollection<SarUnitRow> HostUnits { get; set; }

    public Training_Old()
      : base()
    {
      this.County = "King";
      this.StartTime = DateTime.Now.Date;
      this.Roster = new List<TrainingRoster_Old>();
      this.OfferedCourses = new List<TrainingCourse>();
      this.HostUnits = new List<SarUnitRow>();
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", this.StateNumber, this.Title);
    }

    bool stopDirty = false;
    bool startDirty = false;

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> Start:{1}, County:{2}, Completed:{3}", this.Title, this.StartTime, this.StateNumber, this.StopTime);
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

      if (!string.IsNullOrEmpty(this.StateNumber) && !Regex.IsMatch(this.StateNumber, @"\d{2}\-T-\d{4}"))
      {
        yield return new ValidationResult("Must be in form 00-T-0000", new[] { "StateNumber" });
      }
    }

    #region IRosterEvent<Training,TrainingRoster> Members

    IEnumerable<TrainingRoster_Old> IRosterEvent<Training_Old, TrainingRoster_Old>.Roster
    {
      get { return (IEnumerable<TrainingRoster_Old>)this.Roster; }
    }

    #endregion
  }
}
