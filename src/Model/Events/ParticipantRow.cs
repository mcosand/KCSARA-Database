/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("Participants")]
  public class ParticipantRow : ModelObjectRow
  {
    public ParticipantRow() : base()
    {
    }

    public ParticipantRow(MemberRow m, SarEventRow e)
      : this()
    {
      this.Member = m;
      this.MemberId = m.Id;
      this.Firstname = m.FirstName;
      this.Lastname = m.LastName;
      this.WorkerNumber = m.DEM;
      this.Event = e;
      this.EventId = e.Id;
    }

    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string WorkerNumber { get; set; }

    [ForeignKey("EventId")]
    public virtual SarEventRow Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public Guid? MemberId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> / {1} {2}", this.Event.Title, this.Firstname, this.Lastname);
    }
  }
}
