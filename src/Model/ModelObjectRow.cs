/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public abstract class ModelObjectRow : IModelObjectRow
  {
    public DateTime LastChanged { get; set; }
    public string ChangedBy { get; set; }
    public Guid Id { get; set; }

    public ModelObjectRow()
    {
      this.Id = Guid.NewGuid();
    }

    public override string ToString()
    {
      return GetReportHtml();
    }

    public abstract string GetReportHtml();

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      return new ValidationResult[0];
    }
  }
}
