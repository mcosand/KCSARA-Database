/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("MissionRosters")]
  public class MissionRoster_Old : ModelObject, IRosterEntry<Mission_Old, MissionRoster_Old>, IRosterEntry, IModelObject
  {
    public const string ROLE_FIELD = "Field";
    public const string ROLE_BASE = "Base";
    public const string ROLE_NO_ROLE = "Responder";
    public const string ROLE_IN_TOWN = "InTown";
    public const string ROLE_UNIT_LEAD = "OL";
    public static readonly string[] RoleTypes = new string[] { ROLE_FIELD, ROLE_UNIT_LEAD, ROLE_BASE, ROLE_IN_TOWN, ROLE_NO_ROLE };

    public string InternalRole { get; set; }
    public DateTime? TimeIn { get; set; }
    public DateTime? TimeOut { get; set; }
    public int? Miles { get; set; }
    public string Comments { get; set; }

    [Required]
    public virtual Mission_Old Mission { get; set; }
    [Required]
    public virtual Member Person { get; set; }
    [Required]
    public virtual SarUnit Unit { get; set; }
    public virtual ICollection<AnimalMission> Animals { get; set; }
    public double? OvertimeHours { get; set; }

    public MissionRoster_Old()
      : base()
    {
      this.Animals = new List<AnimalMission>();
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

    public IRosterEvent<Mission_Old, MissionRoster_Old> GetRosterEvent()
    {
      return this.Mission;
    }

    public IRosterEvent GetEvent()
    {
      return this.Mission;
    }

    public void SetEvent(IRosterEvent sarEvent)
    {
      // Passing a non-mission IRosterEvent here will (and should) throw a cast exception.
      this.Mission = (Mission_Old)sarEvent;
    }

    bool timeInDirty = false;
    bool timeOutDirty = false;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (!MissionRoster_Old.RoleTypes.Contains(this.InternalRole))
      {
        yield return new ValidationResult("Must be one of '" + string.Join(",", MissionRoster_Old.RoleTypes) + "'", new[] { "Role" });
      }

      foreach (var result in ValidateTimeInOut())
      {
        yield return result;
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

        if (this.OvertimeHours.HasValue && this.OvertimeHours.Value < 0)
        {
          yield return new ValidationResult("Overtime hours must be >= 0", new[] { "OvertimeHours" });
        }

        // Load the Person again, along with all the mission rosters, so we can search through them...

        //  InA    InThis  OutThis    OutA
        //  InA    InThis   OutA    OutThis
        //  InThis   InA    OutThis    OutA
        //  InThis   InA    OutA     OutThis

        foreach (MissionRoster_Old row in this.Person.MissionRosters)
        {
          if (row.Id == this.Id || !row.TimeOut.HasValue/* || row.EntityState == System.Data.EntityState.Deleted*/)
          {
            continue;
          }

          if ((row.TimeIn <= this.TimeIn && row.TimeOut.Value > this.TimeIn) || (this.TimeIn <= row.TimeIn && this.TimeOut.Value > row.TimeIn))
          {
            yield return new ValidationResult(string.Format("Conflicts with another roster entry, where TimeIn={0} and TimeOut={1}", row.TimeIn, row.TimeOut), new[] { field });
          }
        }
      }
    }

    public override string GetReportHtml()
    {
      return string.Format("[<b>{0}</b>] [<b>{1}</b>] In:{2}, Out:{3}, Miles:{4}", this.Mission.Title, this.Person.FullName, this.TimeIn, this.TimeOut, this.Miles);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }
  }
}
