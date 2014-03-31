/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class MissionLog : ModelObject
  {
    public DateTime Time { get; set; }
    public string Data { get; set; }
    public virtual Mission Mission { get; set; }
    public virtual Member Person { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> @{1}: {2} [{3}]", this.Mission.Title, this.Time, this.Data, (this.Person == null) ? "unknown" : this.Person.FullName);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (string.IsNullOrEmpty(this.Data))
      {
        yield return new ValidationResult("Required", new[] { "Data" });
      }

      if (this.Time < KcsarContext.MinEntryDate)
      {
        yield return new ValidationResult("Must be after " + KcsarContext.MinEntryDate.ToString(), new[] { "Time" });
      }
    }
  }
}
