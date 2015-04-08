/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Data.Events;

  [Table("Units")]
  public class UnitRow : ModelObjectRow
  {
    [Required]
    public string DisplayName { get; set; }

    public string LongName { get; set; }
    public string County { get; set; }
    public string Comments { get; set; }
    public virtual ICollection<ParticipatingUnitRow> Participation { get; set; }
    public virtual ICollection<TrainingCourseRow> TrainingCourses { get; set; }
    public virtual ICollection<UnitMembershipRow> Memberships { get; set; }
    public virtual ICollection<UnitStatusRow> StatusTypes { get; set; }
    public bool HasOvertime { get; set; }

    [Required]
    public string NoApplicationsText { get; set; }
    public virtual ICollection<UnitDocumentRow> Documents { get; set; }
    public virtual ICollection<UnitContactRow> Contacts { get; set; }
    public virtual ICollection<UnitApplicantRow> Applicants { get; set; }

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
