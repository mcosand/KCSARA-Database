/*
 * Copyright 2013-2016 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class UnitApplicant : ModelObject
  {
    [ReportedReference]
    public virtual MemberRow Applicant { get; set; }
    [ReportedReference]
    public virtual SarUnitRow Unit { get; set; }
    public DateTime Started { get; set; }
    public string Data { get; set; }
    public bool IsActive { get; set; }

    public UnitApplicant()
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
