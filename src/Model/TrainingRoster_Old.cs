/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("TrainingRosters")]
  public class TrainingRoster_Old : ModelObject, IRosterEntry<Training_Old, TrainingRoster_Old>
  {
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public int? Miles { get; set; }
    public string Comments { get; set; }
    public virtual Member Person { get; set; }
    public virtual Training_Old Training { get; set; }
    public virtual ICollection<TrainingAward> TrainingAwards { get; set; }
    public virtual ICollection<ComputedTrainingAward> ComputedAwards { get; set; }
    public int? OvertimeHours { get; set; }

    public TrainingRoster_Old()
    {
      this.TrainingAwards = new List<TrainingAward>();
      this.ComputedAwards = new List<ComputedTrainingAward>();
    }

    public double? Hours
    {
      get
      {
        if (this.TimeIn == null || this.TimeOut == null)
        {
          return null;
        }

        return (this.TimeOut - this.TimeIn).Value.TotalHours;
      }
    }

    bool timeInDirty = false;
    bool timeOutDirty = false;

    public IRosterEvent<Training_Old, TrainingRoster_Old> GetRosterEvent()
    {
      return this.Training;
    }

    public Func<TrainingRoster_Old, Training_Old> RosterToEventFunc
    {
      get { return x => x.Training; }
    }

    public IRosterEvent GetEvent()
    {
      return this.Training;
    }

    public void SetEvent(IRosterEvent sarEvent)
    {
      // Passing a non-training IRosterEvent here will (and should) throw a cast exception.
      this.Training = (Training_Old)sarEvent;
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      foreach (var result in ValidateTimeInOut())
      {
        yield return result;
      }

      if (this.Miles.HasValue && this.Miles.Value < 0)
      {
        yield return new ValidationResult("Invalid", new[] { "Miles" });
      }
    }

    private IEnumerable<ValidationResult> ValidateTimeInOut()
    {
      if (!this.TimeIn.HasValue)
      {
        yield return new ValidationResult("Time In is required", new[] { "TimeIn" });
        yield break;
      }

      if (this.TimeOut.HasValue)
      {
        string field = (timeInDirty && !timeOutDirty) ? "TimeIn" : "TimeOut";
        string badValue = ((timeInDirty && !timeOutDirty) ? this.TimeIn.Value : this.TimeOut.Value).ToString("MM/dd/yy HH:mm");

        if (this.TimeOut <= this.TimeIn)
        {
          yield return new ValidationResult("In must be before Out", new[] { field });
          yield break;
        }

        // Load the Person again, along with all the Training rosters, so we can search through them...

        //  InA    InThis  OutThis    OutA
        //  InA    InThis   OutA    OutThis
        //  InThis   InA    OutThis    OutA
        //  InThis   InA    OutA     OutThis

        foreach (TrainingRoster_Old row in this.Person.TrainingRosters)
        {
          if (row.Id == this.Id || !row.TimeOut.HasValue/* || row.EntityState == System.Data.EntityState.Deleted*/)
          {
            continue;
          }

          if ((row.TimeIn <= this.TimeIn && row.TimeOut.Value > this.TimeIn) || (this.TimeIn <= row.TimeIn && this.TimeOut.Value > row.TimeIn))
          {
            yield return new ValidationResult(string.Format("Conflicts with another roster entry, where TimeIn={0} and TimeOut={1}", row.TimeIn, row.TimeOut), new[]{field});
          }
        }
      }
    }

    public override string GetReportHtml()
    {
      return string.Format("[<b>{0}</b>] [<b>{1}</b>] In:{2}, Out:{3}, Miles:{4}", this.Training.Title, this.Person.FullName, this.TimeIn, this.TimeOut, this.Miles);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }
  }
}
