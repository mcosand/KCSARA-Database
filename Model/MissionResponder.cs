/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq.Expressions;

  public class MissionResponder : ModelObject
  {
    public const string ReportFormat = "<b>{0} {1}</b> responded to {2} {3} with {4}";

    [Required]
    public virtual Mission Mission { get; set; }
    [ForeignKey("Mission")]
    public Guid MissionId { get; set; }

    [Required]
    public virtual MissionRespondingUnit RespondingUnit { get; set; }
    [ForeignKey("RespondingUnit")]
    public Guid RespondingUnitId { get; set; }

    public virtual Member Member { get; set; }
    [ForeignKey("Member")]
    public Guid? MemberId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string WorkerNumber { get; set; }

    public Expression<Func<MissionResponder, bool>> IsVisitorPredicate =
      m => m.Member == null;

    public bool IsVisitor
    {
      get
      {
        return this.Member == null;
      }
    }

    public decimal? Hours { get; set; }
    public int? Miles { get; set; }
    public string Role { get; set; }

    public virtual ICollection<MissionRoster> RosterEntries { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(ReportFormat,
        this.FirstName ?? this.Member.FirstName,
        this.LastName ?? this.Member.LastName,
        this.Mission.StateNumber,
        this.Mission.Title,
        this.RespondingUnit.Name ?? this.RespondingUnit.Unit.DisplayName);
    }
  }
}
