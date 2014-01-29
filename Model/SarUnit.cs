/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class SarUnit : ModelObject
  {
    [Required]
    public string DisplayName { get; set; }

    public string LongName { get; set; }
    public string County { get; set; }
    public string Comments { get; set; }
    public virtual ICollection<MissionRoster> MissionRosters { get; set; }
    public virtual ICollection<MissionRespondingUnit> MissionResponses { get; set; } 
    public virtual ICollection<TrainingCourse> TrainingCourses { get; set; }
    public virtual ICollection<UnitMembership> Memberships { get; set; }
    public virtual ICollection<UnitStatus> StatusTypes { get; set; }
    public bool HasOvertime { get; set; }

    public string NoApplicationsText { get; set; }
    public virtual ICollection<UnitDocument> Documents { get; set; }
    public virtual ICollection<UnitContact> Contacts { get; set; }
    public virtual ICollection<UnitApplicant> Applicants { get; set; }

    public SarUnit()
    {
      this.MissionRosters = new List<MissionRoster>();
      this.MissionResponses = new List<MissionRespondingUnit>();
      this.TrainingCourses = new List<TrainingCourse>();
      this.Memberships = new List<UnitMembership>();
      this.StatusTypes = new List<UnitStatus>();
      this.Documents = new List<UnitDocument>();
      this.Contacts = new List<UnitContact>();
      this.Applicants = new List<UnitApplicant>();
    }

    public override string ToString()
    {
      return this.DisplayName;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> ({1}) County:{2}", this.DisplayName, this.LongName, this.County);
    }
    #region IValidatedEntity Members

    public override bool Validate()
    {
      errors.Clear();

      return (errors.Count == 0);
    }

    #endregion
  }
}
