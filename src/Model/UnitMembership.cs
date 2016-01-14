/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;

  public class UnitMembership : ModelObject
  {
    public DateTime Activated { get; set; }
    public string Comments { get; set; }
    public virtual Member Person { get; set; }
    public virtual SarUnitRow Unit { get; set; }
    public virtual UnitStatus Status { get; set; }
    public DateTime? EndTime { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b>, {1} member of <b>{2}</b> since {3:d}", this.Person.FullName, this.Status.StatusName, this.Unit, this.Activated);
    }

    public override string ToString()
    {
      return string.Format("{0} [{1}][{2}", this.Person.FullName, this.Unit, this.Status.StatusName);
    }
  }
}
