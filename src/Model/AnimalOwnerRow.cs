/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("AnimalOwners")]
  public class AnimalOwnerRow : ModelObjectRow
  {
    public const string ReportFormat = "<b>{0}</b>, owned by {1} (primary={2}) from {3} - {4}";

    public bool IsPrimary { get; set; }
    public DateTime Starting { get; set; }
    public DateTime? Ending { get; set; }
    
    [ForeignKey("AnimalId")]
    public virtual AnimalRow Animal { get; set; }
    public Guid AnimalId { get; set; }

    [ForeignKey("OwnerId")]
    public virtual MemberRow Owner { get; set; }
    public Guid OwnerId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalOwnerRow.ReportFormat, this.Animal.Name, this.Owner.FullName, this.IsPrimary, this.Starting, this.Ending);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (this.Ending.HasValue && this.Ending <= this.Starting)
      {
        yield return new ValidationResult("Ending date must be after start date", new[] { "Ending", "Starting" });
      }
    }
  }
}
