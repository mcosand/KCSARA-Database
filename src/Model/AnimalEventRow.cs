/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Data.Events;

  [Table("AnimalEvents")]
  public class AnimalEventRow : ModelObjectRow
  {
    public const string ReportFormat = "<b>[{0}] [{1}]</b>";

    [ForeignKey("AnimalId")]
    public virtual AnimalRow Animal { get; set; }
    public Guid AnimalId { get; set; }

    [ForeignKey("RosterId")]
    public virtual EventRosterRow Roster { get; set; }
    public Guid RosterId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalEventRow.ReportFormat, this.Roster.Event.Title, this.Animal.Name);
    }
  }
}
