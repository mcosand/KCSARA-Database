/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("SubjectGroupLinks")]
  public class SubjectGroupLinkRow : ModelObjectRow
  {
    public int Number { get; set; }
    [ForeignKey("SubjectId")]
    public virtual SubjectRow Subject { get; set; }
    public Guid SubjectId { get; set; }

    [ForeignKey("GroupId")]
    public virtual SubjectGroupRow Group { get; set; }
    public Guid GroupId { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}'s behavior on {1}</b> ", this.Subject.FirstName, this.Group.Event.Title);
    }
  }
}
