/*
 * Copyright 2015 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model.Events
{
  public class ParticipantStatus : ParticipantEventTimeline
  {
    public ParticipatingUnit Unit { get; set; }
    public Guid? UnitId { get; set; }

    public ParticipantStatusType Status { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0} / {1} {2}</b> {3} with {4} @{5}", this.Event.Title, this.Participant.Firstname, this.Participant.Lastname, this.Status, this.Unit.Nickname, this.Time);
    }
  }

  public enum ParticipantStatusType
  {
    Unknown,
    Unavailable,
    Activated,
    Responding,
    SignedIn,
    Assigned,
    RestTime,
    SignedOut,
    Cleared
  }

}
