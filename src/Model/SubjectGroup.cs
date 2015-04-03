/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Kcsar.Database.Model.Events;

  public class SubjectGroup : ModelObject
  {
    public int Number { get; set; }
    public DateTime? WhenLost { get; set; }
    public DateTime? WhenReported { get; set; }
    public DateTime? WhenCalled { get; set; }
    public DateTime? WhenAtPls { get; set; }
    public DateTime? WhenFound { get; set; }
    public string PlsEasting { get; set; }
    public string PlsNorthing { get; set; }
    public int? PlsCertainty { get; set; }
    public int? PlsElevation { get; set; }
    public string PlsCommonName { get; set; }
    public string FoundEasting { get; set; }
    public string FoundNorthing { get; set; }
    public int? FoundCertainty { get; set; }
    public string FoundCondition { get; set; }
    public int? FoundElevation { get; set; }
    public string FoundTactics { get; set; }
    public string Category { get; set; }
    public string Cause { get; set; }
    public string Behavior { get; set; }
    public string Comments { get; set; }
    public virtual Mission Mission { get; set; }
    [ForeignKey("EventId")]
    public virtual SarEvent Event { get; set; }
    public Guid? EventId { get; set; }

    public virtual ICollection<SubjectGroupLink> SubjectLinks { get; set; }

    public SubjectGroup()
      : base()
    {
      this.SubjectLinks = new List<SubjectGroupLink>();
    }

    public SubjectGroup CreateCopy()
    {
      SubjectGroup n = new SubjectGroup();
      n.WhenAtPls = this.WhenAtPls;
      n.WhenCalled = this.WhenCalled;
      n.WhenFound = this.WhenFound;
      n.WhenLost = this.WhenLost;
      n.WhenReported = this.WhenReported;
      n.Category = this.Category;
      n.Cause = this.Cause;
      n.FoundCertainty = this.FoundCertainty;
      n.FoundCondition = this.FoundCondition;
      n.FoundEasting = this.FoundEasting;
      n.FoundElevation = this.FoundElevation;
      n.FoundNorthing = this.FoundNorthing;
      n.FoundTactics = this.FoundTactics;
      n.Event = this.Event;
      n.EventId = this.EventId;
      n.PlsCertainty = this.PlsCertainty;
      n.PlsCommonName = this.PlsCommonName;
      n.PlsEasting = this.PlsEasting;
      n.PlsElevation = this.PlsElevation;
      n.PlsNorthing = this.PlsNorthing;
      return n;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0} Subjects Group {1}</b> ", this.Event.Title, this.Number);
    }
  }
}
