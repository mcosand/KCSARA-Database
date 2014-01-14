
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class Mission : ModelObject, IRosterEvent<Mission, MissionRoster>
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

    public virtual ICollection<Mission> Followups { get; set; }
    public virtual Mission Previous { get; set; }

    public virtual ICollection<MissionLog> Log { get; set; }
    public virtual ICollection<MissionRoster> Roster { get; set; }

    public virtual MissionDetails Details { get; set; }
    public virtual ICollection<SubjectGroup> SubjectGroups { get; set; }
    public virtual ICollection<MissionGeography> MissionGeography { get; set; }

    public virtual MissionResponseStatus ResponseStatus { get; set; }

    public Mission()
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

    //partial void OnStartTimeChanging(DateTime value)
    //{
    //    startDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.StartTime);
    //}

    //partial void OnStopTimeChanging(DateTime? value)
    //{
    //    stopDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.StopTime);
    //}

    public override string GetReportHtml()
    {
      //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
      //{
      //    if (!this.PreviousReference.IsLoaded)
      //    {
      //        this.PreviousReference.Load();
      //    }
      //}
      return string.Format("{0} <b>{1}</b> Start:{2}, Type:{3}, County:{4}, Completed:{5}", this.StateNumber, this.Title, this.StartTime, this.MissionType, this.County, this.StopTime);
    }

    IEnumerable<MissionRoster> IRosterEvent<Mission, MissionRoster>.Roster
    {
      get { return (IEnumerable<MissionRoster>)this.Roster; }
    }

    #region IValidatedEntity Members

    public override bool Validate()
    {
      errors.Clear();

      if (string.IsNullOrEmpty(this.Title))
      {
        errors.Add(new RuleViolation(this.Id, "Title", "", "Required"));
      }

      if (string.IsNullOrEmpty(this.Location))
      {
        errors.Add(new RuleViolation(this.Id, "Location", "", "Required"));
      }

      if (string.IsNullOrEmpty(this.County))
      {
        errors.Add(new RuleViolation(this.Id, "County", "", "Required"));
      }
      else if (!Utils.CountyNames.Contains(this.County, new Utils.CaseInsensitiveComparer()))
      {
        errors.Add(new RuleViolation(this.Id, "County", this.County, "Not a county in Washington"));
      }


      if (this.StopTime.HasValue)
      {
        string field = (startDirty && !stopDirty) ? "StartTime" : "StopTime";
        string badValue = ((startDirty && !stopDirty) ? this.StartTime : this.StopTime.Value).ToString("MM/dd/yy HH:mm");

        if (this.StopTime <= this.StartTime)
        {
          errors.Add(new RuleViolation(this.Id, field, badValue, "Start must be before Stop"));
        }
      }

      if (!string.IsNullOrEmpty(this.StateNumber) && !Regex.IsMatch(this.StateNumber, @"^\d{2}\-\d{4}$") && !Regex.IsMatch(this.StateNumber, @"^\d{2}\-ES\-\d{3}$", RegexOptions.IgnoreCase))
      {
        errors.Add(new RuleViolation(this.Id, "StateNumber", this.StateNumber, "Must be in form 00-0000 or 00-ES-000"));
      }

      if (!string.IsNullOrEmpty(this.CountyNumber) && this.County.Equals("King", StringComparison.OrdinalIgnoreCase) && !Regex.IsMatch(this.CountyNumber, @"^\d{2}\-\d{6}$"))
      {
        errors.Add(new RuleViolation(this.Id, "CountyNumber", this.CountyNumber, "Must be in form 00-000000"));
      }

      if (this.Previous != null && this.Previous.Id == this.Id)
      {
        errors.Add(new RuleViolation(this.Id, "Previous", "", "Can't link a mission to itself"));
      }

      return (errors.Count == 0);
    }

    #endregion
  }
}
