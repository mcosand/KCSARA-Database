/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class Subject : ModelObject
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
    public virtual ICollection<SubjectGroupLink> GroupLinks { get; set; }

    public Subject()
      : base()
    {
      this.Gender = Gender.Unknown;
      this.GroupLinks = new List<SubjectGroupLink>();
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
