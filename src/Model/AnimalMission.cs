/*
 * Copyright 2009-2014 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Kcsar.Database.Model.Events;
namespace Kcsar.Database.Model
{

  public class AnimalMission : ModelObject
  {
    public const string ReportFormat = "<b>[{0}] [{1}]</b>";

    public virtual Animal Animal { get; set; }
    public virtual MissionRoster MissionRoster { get; set; }
    
    [ForeignKey("RosterId")]
    public virtual EventRoster MissionRoster2 { get; set; }
    public Guid? RosterId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalMission.ReportFormat, this.MissionRoster2.Event.Title, this.Animal.Name);
    }
  }
}
