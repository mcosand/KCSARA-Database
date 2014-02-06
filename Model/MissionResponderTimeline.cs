/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq.Expressions;

  public class MissionResponderTimelime : ModelObject
  {
    public const string ReportFormat = "<b>{0} {1}</b>, {2} at {3}";

    [Required]
    public virtual MissionResponder Responder { get; set; }
    [ForeignKey("Responder")]
    public Guid ResponderId { get; set; }

    public DateTime Time { get; set; }

    public ResponderStatus Status { get; set; }
    public DbGeography Location { get; set; }
    public DateTime? Eta { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(ReportFormat,
        this.Responder.FirstName ?? this.Responder.Member.FirstName,
        this.Responder.LastName ?? this.Responder.Member.LastName,
        this.Status,
        this.Time);
    }
  }
}
