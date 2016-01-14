/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model.Events
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventUnits")]
  public class EventUnitRow : ModelObject
  {
    public string Name { get; set; }

    public string County { get; set; }

    public Guid? MemberUnitId { get; set; }

    [ForeignKey("MemberUnitId")]
    public SarUnitRow MemberUnit { get; set; }

    public Guid? EventId { get; set; }

    [ForeignKey("EventId")]
    public SarEventRow Event { get; set; }

    public virtual ICollection<EventParticipantRow> Participants { get; set; }

    public override string GetReportHtml()
    {
      return "NYI";
    }
  }
}
