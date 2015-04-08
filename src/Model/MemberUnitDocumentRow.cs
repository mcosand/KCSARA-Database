/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Model;

  [Table("MemberUnitDocuments")]
  public class MemberUnitDocumentRow : ModelObjectRow
  {
    [Required]
    [ReportedReference]
    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public Guid MemberId { get; set; }

    [Required]
    [ReportedReference]
    [ForeignKey("DocumentId")]
    public virtual UnitDocumentRow Document { get; set; }
    public Guid DocumentId { get; set; }

    public DateTime? MemberAction { get; set; }

    public DateTime? UnitAction { get; set; }

    public DocumentStatus Status { get; set; }

    public override string ToString()
    {
      return string.Format("{0}'s unit document: {1}/{2}", this.Member.FullName, this.Document.Unit.DisplayName, this.Document.Title);
    }

    public override string GetReportHtml()
    {
      return string.Format("{0} unit document {1}/<b>{2}</b> {3}", this.Member.FullName, this.Document.Unit.DisplayName, this.Document.Title, this.Status);
    }
  }
}
