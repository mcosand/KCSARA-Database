/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("UnitApplicants")]
  public class UnitApplicantRow : ModelObjectRow
  {
    [ReportedReference]
    [ForeignKey("ApplicantId")]
    public virtual MemberRow Applicant { get; set; }
    public Guid ApplicantId { get; set; }

    [ReportedReference]
    [ForeignKey("UnitId")]
    public virtual UnitRow Unit { get; set; }
    public Guid UnitId { get; set; }

    public DateTime Started { get; set; }
    public string Data { get; set; }
    public bool IsActive { get; set; }

    public UnitApplicantRow()
      : base()
    {
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> applying to <b>{1}</b>, started: {2}, active: {3}", this.Applicant.FullName, this.Unit.DisplayName, this.Started, this.IsActive);
    }
  }
}
