/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Runtime.Serialization;
  using System.Threading;
  
  public class MissionRespondingUnit : ModelObject
  {
    public static readonly string ReportFormat = "Unit '{0}' responding to '{1}'";

    [Required]
    public virtual Mission Mission { get; set; }
    [ForeignKey("Mission")]
    public Guid MissionId { get; set; }

    public virtual SarUnit Unit { get; set; }
    [ForeignKey("Unit")]
    public Guid? UnitId { get; set; } 
    public virtual ICollection<MissionResponder> Responders { get; set; }
    
    public virtual MissionResponder Lead { get; set; }
    [ForeignKey("Lead")]
    public Guid? LeadId { get; set; }
    public string Name { get; set; }
    public string LongName { get; set; }


    public Expression<Func<MissionRespondingUnit, bool>> IsVisitorPredicate =
      m => m.Unit == null;

    public bool IsVisitor
    {
      get
      {
        return !this.UnitId.HasValue;
      }
    }

    public bool IsActive { get; set; }


    public override string GetReportHtml()
    {
      return string.Format(ReportFormat, this.Unit.DisplayName, this.Mission.Title);
    }
  }
}
