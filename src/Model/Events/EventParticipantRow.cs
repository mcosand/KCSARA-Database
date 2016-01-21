/*
 * Copyright 2009-2016 Matthew Cosand
 */
 namespace Kcsar.Database.Model.Events
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventParticipants")]
  public class EventParticipantRow : ModelObject
  {
    public EventParticipantRow()
    {
      Timeline = new List<EventParticipantStatusRow>();
      AwardedTraining = new List<TrainingAwardRow>();
    }

    public Guid EventId { get; set; }

    [ForeignKey("EventId")]
    public SarEventRow Event { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string StateIdNumber { get; set; }

    public Guid? MemberId { get; set; }

    [ForeignKey("MemberId")]
    public MemberRow Member { get; set; }
    
    public Guid? EventUnitId { get; set; }

    [ForeignKey("EventUnitId")]
    public EventUnitRow EventUnit { get; set; }

    public ParticipantStatus LastStatus { get; set; }

    public double? Hours { get; set; }

    public int? Miles { get; set; }

    public virtual ICollection<EventParticipantStatusRow> Timeline { get; set; }

    public virtual ICollection<TrainingAwardRow> AwardedTraining { get; set; }

    public override string GetReportHtml()
    {
      return "NYI";
    }
  }
}
