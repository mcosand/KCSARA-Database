/*
 * Copyright 2008-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("UnitMemberships")]
  public class UnitMembershipRow : ModelObjectRow
  {
    public DateTime Activated { get; set; }
    public string Comments { get; set; }
    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public Guid MemberId { get; set; }

    [ForeignKey("UnitId")]
    public virtual UnitRow Unit { get; set; }
    public Guid UnitId { get; set; }

    [ForeignKey("StatusId")]
    public virtual UnitStatusRow Status { get; set; }
    public Guid StatusId { get; set; }

    public DateTime? EndTime { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b>, {1} member of <b>{2}</b> since {3:d}", this.Member.FullName, this.Status.StatusName, this.Unit, this.Activated);
    }

    public override string ToString()
    {
      return string.Format("{0} [{1}][{2}", this.Member.FullName, this.Unit, this.Status.StatusName);
    }
  }
}
