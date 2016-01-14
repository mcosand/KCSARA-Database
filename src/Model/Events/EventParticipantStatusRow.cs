/*
 * Copyright 2009-2014 Matthew Cosand
 */
 namespace Kcsar.Database.Model.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventParticipantStatus")]
  public class EventParticipantStatusRow : ModelObject
  {
    public Guid ParticipantId { get; set; }

    [ForeignKey("ParticipantId")]
    public EventParticipantRow Participant { get; set; }

    public DateTime Time { get; set; }

    public ParticipantStatus Status { get; set; }

    public string Role { get; set; }

    public Guid? EventUnitId { get; set; }

    [ForeignKey("EventUnitId")]
    public EventUnitRow EventUnit { get; set; }

    public int? Miles { get; set; }

    public override string GetReportHtml()
    {
      return "NYI";
    }
  }
}
