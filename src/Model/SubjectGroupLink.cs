/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class SubjectGroupLink : ModelObject
  {
    public int Number { get; set; }
    public virtual Subject Subject { get; set; }
    public virtual SubjectGroup Group { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}'s behavior on {1}</b> ", this.Subject.FirstName, this.Group.Event.Title);
    }
  }
}
