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
  public class ParticipatingUnit : ModelObject
  {
    public string Nickname { get; set; }
    public string Name { get; set; }

    [ForeignKey("EventId")]
    public SarEvent Event { get; set; }
    public Guid EventId { get; set; }

    [ForeignKey("MemberUnitId")]
    public SarUnit MemberUnit { get; set; }
    public Guid? MemberUnitId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> / {1}", this.Event.Title, this.Nickname);
    }
  }
}
