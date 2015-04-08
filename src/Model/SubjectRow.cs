/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Data.Events;
  using Kcsar.Database.Model;

  [Table("Subjects")]
  public class SubjectRow : ModelObjectRow
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string InternalGender { get; set; }
    public int? BirthYear { get; set; }
    public string Address { get; set; }
    public string HomePhone { get; set; }
    public string WorkPhone { get; set; }
    public string OtherPhone { get; set; }
    public string Comments { get; set; }

    public virtual SarEventRow Event { get; set; }
    public virtual ICollection<SubjectGroupLinkRow> GroupLinks { get; set; }

    public SubjectRow()
      : base()
    {
      this.Gender = Gender.Unknown;
      this.GroupLinks = new List<SubjectGroupLinkRow>();
    }

    [NotMapped]
    public Gender Gender
    {
      get
      {
        Gender result = Gender.Unknown;
        foreach (string s in Enum.GetNames(typeof(Gender)))
        {
          if (string.Equals(this.InternalGender, s.Substring(0, 1), StringComparison.OrdinalIgnoreCase))
          {
            result = (Gender)Enum.Parse(typeof(Gender), s, true);
          }
        }
        return result;
      }

      set
      {
        if (value == Gender.Unknown)
        {
          this.InternalGender = null;
        }
        else
        {
          this.InternalGender = value.ToString().Substring(0, 1).ToLower();
        }
      }
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0} {1}</b> ", this.FirstName, this.LastName);
    }
  }
}
