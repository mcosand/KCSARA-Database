/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;

  [Table("Animals")]
  public class AnimalRow : ModelObjectRow
  {
    public static readonly string[] AllowedTypes = new string[] { "horse", "dog" };
    public static readonly string ReportFormat = "<b>{0}</b> Suffix:{1} Type:{2} Comments:{3}";

    public string DemSuffix { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Comments { get; set; }
    public virtual ICollection<AnimalOwnerRow> Owners { get; set; }
    public virtual ICollection<AnimalEventRow> MissionRosters { get; set; }
    public string PhotoFile { get; set; }

    public AnimalRow()
      : base()
    {
      this.Type = AnimalRow.AllowedTypes[0];
      this.Owners = new List<AnimalOwnerRow>();
      this.MissionRosters = new List<AnimalEventRow>();
    }

    public MemberRow GetPrimaryOwner()
    {
      foreach (AnimalOwnerRow link in this.Owners)
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
      return string.Format(AnimalRow.ReportFormat, this.Name, this.DemSuffix, this.Type, this.Comments);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {

      if (string.IsNullOrEmpty(this.Name))
      {
        yield return new ValidationResult("Required", new[] { "Name" });
      }

      if (string.IsNullOrEmpty(this.DemSuffix))
      {
        yield return new ValidationResult("Required", new[] { "DemSuffix" });
      }

      if (string.IsNullOrEmpty(this.Type))
      {
        yield return new ValidationResult("Required", new[] { "Type" });
      }
      else if (!AnimalRow.AllowedTypes.Contains(this.Type.ToLower()))
      {
        yield return new ValidationResult("Must be one of: " + string.Join(", ", AnimalRow.AllowedTypes), new[] { "Type" });
      }

    }
  }
}
