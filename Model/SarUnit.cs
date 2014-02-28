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
    public virtual ICollection<TrainingCourse> TrainingCourses { get; set; }
    public virtual ICollection<UnitMembership> Memberships { get; set; }
    public virtual ICollection<UnitStatus> StatusTypes { get; set; }
    public bool HasOvertime { get; set; }

    public string NoApplicationsText { get; set; }
    public virtual ICollection<UnitDocument> Documents { get; set; }
    public virtual ICollection<UnitContact> Contacts { get; set; }
    public virtual ICollection<UnitApplicant> Applicants { get; set; }

    public override string ToString()
    {
      return this.DisplayName;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> ({1}) County:{2}", this.DisplayName, this.LongName, this.County);
    }
  }
}
