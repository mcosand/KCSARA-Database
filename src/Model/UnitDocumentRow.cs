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

  [Table("UnitDocuments")]
  public class UnitDocumentRow : ModelObjectRow
  {
    [Required]
    public string Url { get; set; }

    [Required]
    public string Title { get; set; }

    [ReportedReference]
    [ForeignKey("UnitId")]
    public virtual UnitRow Unit { get; set; }
    public Guid UnitId { get; set; }

    [Required]
    public int Order { get; set; }

    public UnitDocumentType Type { get; set; }

    public string SubmitTo { get; set; }

    public bool Required { get; set; }

    public int? ForMembersOlder { get; set; }
    public int? ForMembersYounger { get; set; }

    public override string ToString()
    {
      return this.Unit.DisplayName + "'s " + this.Title;
    }

    public override string GetReportHtml()
    {
      return string.Format("{0} application document <b>{1}</b>", this.Unit.DisplayName, this.Title);
    }
  }
}
