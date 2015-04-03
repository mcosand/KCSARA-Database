/*
 * Copyright 2015 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsar.Database.Model.Events
{
  public class EventRoster
  {
    public Guid Id { get; set; }

    public EventRoster()
      : base()
    {
      this.Id = Guid.NewGuid();
    }

    [ForeignKey("EventId")]
    public SarEvent Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("ParticipantId")]
    public Participant Participant { get; set; }
    public Guid ParticipantId { get; set; }

    [ForeignKey("UnitId")]
    public ParticipatingUnit Unit { get; set; }
    public Guid? UnitId { get; set; }

    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
