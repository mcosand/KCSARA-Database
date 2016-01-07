/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventLogs")]
  public class EventLogRow : ModelObject
  {
    public DateTime Time { get; set; }
    public string Data { get; set; }

    public Guid EventId { get; set; }

    [ForeignKey("EventId")]
    public virtual SarEventRow Event { get; set; }

    public Guid? PersonId { get; set; }

    [ForeignKey("PersonId")]
    public virtual Member Person { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> @{1}: {2} [{3}]", this.Event?.Title, this.Time, this.Data, (this.Person == null) ? "unknown" : this.Person.FullName);
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
