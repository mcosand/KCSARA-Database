﻿/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class AnimalOwner : ModelObject
  {
    public const string ReportFormat = "<b>{0}</b>, owned by {1} (primary={2}) from {3} - {4}";

    public bool IsPrimary { get; set; }
    public DateTime Starting { get; set; }
    public DateTime? Ending { get; set; }
    public virtual Animal Animal { get; set; }
    public virtual Member Owner { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalOwner.ReportFormat, this.Animal.Name, this.Owner.FullName, this.IsPrimary, this.Starting, this.Ending);
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
