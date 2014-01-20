/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class Animal : ModelObject
  {
    public static readonly string[] AllowedTypes = new string[] { "horse", "dog" };
    public static readonly string ReportFormat = "<b>{0}</b> Suffix:{1} Type:{2} Comments:{3}";

    public string DemSuffix { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Comments { get; set; }
    public virtual ICollection<AnimalOwner> Owners { get; set; }
    public virtual ICollection<AnimalMission> MissionRosters { get; set; }
    public string PhotoFile { get; set; }

    public Animal()
      : base()
    {
      this.Type = Animal.AllowedTypes[0];
      this.Owners = new List<AnimalOwner>();
      this.MissionRosters = new List<AnimalMission>();
    }

    public Member GetPrimaryOwner()
    {
      foreach (AnimalOwner link in this.Owners)
      {
        if (link.IsPrimary)
        {
          return link.Owner;
        }
      }
      return null;
    }

    public override string GetReportHtml()
    {
      return string.Format(Animal.ReportFormat, this.Name, this.DemSuffix, this.Type, this.Comments);
    }

    #region IValidatedEntity Members

    public override bool Validate()
    {
      errors.Clear();

      if (string.IsNullOrEmpty(this.Name))
      {
        errors.Add(new RuleViolation(this.Id, "Name", "", "Required"));
      }

      if (string.IsNullOrEmpty(this.DemSuffix))
      {
        errors.Add(new RuleViolation(this.Id, "DemSuffix", "", "Required"));
      }

      if (string.IsNullOrEmpty(this.Type))
      {
        errors.Add(new RuleViolation(this.Id, "Type", "", "Required"));
      }
      else if (!Animal.AllowedTypes.Contains(this.Type.ToLower()))
      {
        errors.Add(new RuleViolation(this.Id, "Type", this.Type, "Must be one of: " + string.Join(", ", Animal.AllowedTypes)));
      }

      return (errors.Count == 0);
    }

    #endregion
  }
}
