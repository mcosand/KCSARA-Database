/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class MissionRoster : ModelObject, IRosterEntry<Mission, MissionRoster>, IRosterEntry, IModelObject
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
    public virtual Mission Mission { get; set; }
    [Required]
    public virtual Member Person { get; set; }
    [Required]
    public virtual SarUnit Unit { get; set; }

    public virtual MissionResponder Responder { get; set; }

    public virtual ICollection<AnimalMission> Animals { get; set; }
    public double? OvertimeHours { get; set; }

    public MissionRoster()
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

    public IRosterEvent<Mission, MissionRoster> GetRosterEvent()
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
      this.Mission = (Mission)sarEvent;
    }

    bool timeInDirty = false;
    bool timeOutDirty = false;

    //partial void OnTimeInChanging(DateTime? value)
    //{
    //    timeInDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.TimeIn);
    //}

    //partial void OnTimeOutChanging(DateTime? value)
    //{
    //    timeOutDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.TimeOut);
    //}

    public override bool Validate()
    {
      errors.Clear();

      //if (this.Mission == null && this.MissionReference.EntityKey == null)
      //{
      //    errors.Add(new RuleViolation(this.Id, "Mission", "", "Required"));
      //}

      //if (this.Unit == null && this.UnitReference.EntityKey == null)
      //{
      //    errors.Add(new RuleViolation(this.Id, "Unit", "", "Required"));
      //}

      //if (this.Miles.HasValue && this.Miles.Value < 0)
      //{
      //    errors.Add(new RuleViolation(this.Id, "Miles", this.Miles.Value.ToString(), "Invalid"));
      //}

      //if (this.Person == null && this.PersonReference.EntityKey == null)
      //{
      //    errors.Add(new RuleViolation(this.Id, "Person", "", "Required"));

      //    //  Other validations will fail when this reference is null. Short circuit here.
      //    return false;
      //}

      if (!MissionRoster.RoleTypes.Contains(this.InternalRole))
      {
        errors.Add(new RuleViolation(this.Id, "Role", this.InternalRole, "Must be one of '" + string.Join(",", MissionRoster.RoleTypes) + "'"));
      }

      ValidateTimeInOut();

      return (errors.Count == 0);
    }

    private bool ValidateTimeInOut()
    {
      bool result = true;

      if (!this.TimeIn.HasValue)
      {
        errors.Add(new RuleViolation(this.Id, "TimeIn", "", "Time In is required"));
        return false;
      }

      if (this.TimeOut.HasValue)
      {
        string field = (timeInDirty && !timeOutDirty) ? "TimeIn" : "TimeOut";
        string badValue = ((timeInDirty && !timeOutDirty) ? this.TimeIn.Value : this.TimeOut.Value).ToString("MM/dd/yy HH:mm");

        if (this.TimeOut <= this.TimeIn)
        {
          errors.Add(new RuleViolation(this.Id, field, badValue, "In must be before Out"));
          return false;
        }

        if (this.OvertimeHours.HasValue && this.OvertimeHours.Value < 0)
        {
          errors.Add(new RuleViolation(this.Id, "OvertimeHours", this.OvertimeHours.ToString(), "Overtime hours must be >= 0"));
        }

        // Load the Person again, along with all the mission rosters, so we can search through them...

        //  InA    InThis  OutThis    OutA
        //  InA    InThis   OutA    OutThis
        //  InThis   InA    OutThis    OutA
        //  InThis   InA    OutA     OutThis


        //if (this.Person == null && !this.PersonReference.IsLoaded)
        //{
        //    this.PersonReference.Load();
        //}
        //if (!this.Person.MissionRosters.IsLoaded)
        //{
        //    this.Person.MissionRosters.Load();
        //}

        foreach (MissionRoster row in this.Person.MissionRosters)
        {
          if (row.Id == this.Id || !row.TimeOut.HasValue/* || row.EntityState == System.Data.EntityState.Deleted*/)
          {
            continue;
          }

          if ((row.TimeIn <= this.TimeIn && row.TimeOut.Value > this.TimeIn) || (this.TimeIn <= row.TimeIn && this.TimeOut.Value > row.TimeIn))
          {
            errors.Add(new RuleViolation(this.Id, field, badValue,
                string.Format("Conflicts with another roster entry, where TimeIn={0} and TimeOut={1}", row.TimeIn, row.TimeOut)));
            result = false;
          }
        }
      }
      return result;
    }

    private void ClearErrors(string property)
    {
      for (int i = 0; i < errors.Count; i++)
      {
        if (errors[i].PropertyName == property)
        {
          errors.RemoveAt(i);
          i--;
        }
      }
    }

    public override string GetReportHtml()
    {
      //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
      //{
      //    if (!this.PersonReference.IsLoaded)
      //    {
      //        this.PersonReference.Load();
      //    }

      //    if (!this.MissionReference.IsLoaded)
      //    {
      //        this.MissionReference.Load();
      //    }
      //}
      return string.Format("[<b>{0}</b>] [<b>{1}</b>] In:{2}, Out:{3}, Miles:{4}", this.Mission.Title, this.Person.FullName, this.TimeIn, this.TimeOut, this.Miles);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }
  }
}
