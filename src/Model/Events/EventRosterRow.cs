/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventRosters")]
  public class EventRosterRow
  {
    public Guid Id { get; set; }

    public EventRosterRow()
      : base()
    {
      this.Id = Guid.NewGuid();
    }

    [ForeignKey("EventId")]
    public SarEventRow Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("ParticipantId")]
    public ParticipantRow Participant { get; set; }
    public Guid ParticipantId { get; set; }

    [ForeignKey("UnitId")]
    public ParticipatingUnitRow Unit { get; set; }
    public Guid? UnitId { get; set; }

    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
