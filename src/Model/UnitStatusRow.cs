/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Model;

  [Table("UnitStatus")]
  public class UnitStatusRow : ModelObjectRow
  {
    [Required]
    public string StatusName { get; set; }
    public WacLevel WacLevel { get; set; }
    public bool IsActive { get; set; }
    public bool GetsAccount { get; set; }
    
    [ForeignKey("UnitId")]
    public virtual UnitRow Unit { get; set; }
    public Guid UnitId { get; set; }

    public virtual ICollection<UnitMembershipRow> Memberships { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("[<b>{0}</b>] [<b>{1}</b>] Active={2}, WAC={3}", this.Unit.DisplayName, this.StatusName, this.IsActive, this.WacLevel);
    }
  }
}
