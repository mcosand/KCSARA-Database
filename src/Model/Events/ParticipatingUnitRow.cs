/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("ParticipatingUnits")]
  public class ParticipatingUnitRow : ModelObjectRow
  {
    public string Nickname { get; set; }
    public string Name { get; set; }

    [ForeignKey("EventId")]
    public SarEventRow Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("MemberUnitId")]
    public UnitRow MemberUnit { get; set; }
    public Guid? MemberUnitId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> / {1}", this.Event.Title, this.Nickname);
    }
  }
}
