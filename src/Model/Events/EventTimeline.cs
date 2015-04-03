/*
 * Copyright 2015 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsar.Database.Model.Events
{
  public abstract class EventTimeline : ModelObject
  {
    [ForeignKey("EventId")]
    public SarEvent Event { get; set; }
    public Guid EventId { get; set; }

    public DateTime Time { get; set; }
    public string JsonData { get; set; }
  }

  public abstract class ParticipantEventTimeline : EventTimeline
  {
    [ForeignKey("ParticipantId")]
    public Participant Participant { get; set; }
    public Guid? ParticipantId { get; set; }
  }
}
