/*
 * Copyright 2009-2014 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Kcsar.Database.Model.Events;
namespace Kcsar.Database.Model
{

  public class AnimalEvents : ModelObject
  {
    public const string ReportFormat = "<b>[{0}] [{1}]</b>";

    public virtual Animal Animal { get; set; }

    [ForeignKey("RosterId")]
    public virtual EventRoster Roster { get; set; }
    public Guid RosterId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalEvents.ReportFormat, this.Roster.Event.Title, this.Animal.Name);
    }
  }
}
