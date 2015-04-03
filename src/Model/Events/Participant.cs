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
  public class Participant : ModelObject
  {
    public Participant() : base()
    {
    }

    public Participant(Member m, SarEvent e)
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
    public SarEvent Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("MemberId")]
    public Member Member { get; set; }
    public Guid? MemberId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> / {1} {2}", this.Event.Title, this.Firstname, this.Lastname);
    }
  }
}
