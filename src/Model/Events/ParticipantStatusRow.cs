/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Model;

  // Root table: EventTimelines
  public class ParticipantStatusRow : ParticipantEventTimeline
  {
    [ForeignKey("UnitId")]
    public virtual ParticipatingUnitRow Unit { get; set; }
    public Guid? UnitId { get; set; }

    public ParticipantStatusType Status { get; set; }

    public override string ToMarkdown()
    {
      return string.Format("**{0} {1}** {2}{3}", this.Participant.Firstname, this.Participant.Lastname, this.Status.ToString().ToLower(), this.UnitId == null ? null : (" with " + this.Unit.Nickname));
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0} / {1} {2}</b> {3} with {4} @{5}", this.Event.Title, this.Participant.Firstname, this.Participant.Lastname, this.Status, this.Unit.Nickname, this.Time);
    }
  }
}
