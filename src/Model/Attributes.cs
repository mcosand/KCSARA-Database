/*
 * Copyright 2010-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class ReportingAttribute : Attribute
  {
    public ReportingAttribute()
    {
      this.Hides = "";
      this.Format = "{0}";
    }

    public string Hides { get; set; }
    public string Format { get; set; }
  }

  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class ReportedReferenceAttribute : Attribute
  {

  }

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public sealed class MemberReportingAttribute : Attribute
  {
    public MemberReportingAttribute(string property)
    {
      this.Property = property;
      this.Format = "{0}";
    }

    public string Property { get; private set; }
    public string Format { get; set; }
  }
}
