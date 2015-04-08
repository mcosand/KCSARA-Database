/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventTimelines")]
  public abstract class EventTimelineRow : ModelObjectRow
  {
    [ForeignKey("EventId")]
    public SarEventRow Event { get; set; }
    public Guid EventId { get; set; }

    public DateTime Time { get; set; }
    public string JsonData { get; set; }
  }

  public abstract class ParticipantEventTimeline : EventTimelineRow
  {
    [ForeignKey("ParticipantId")]
    public ParticipantRow Participant { get; set; }
    public Guid? ParticipantId { get; set; }
  }
}
