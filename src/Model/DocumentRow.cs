/*
 * Copyright 2010-2016 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("Documents")]
  public class DocumentRow : ModelObject, IDocument
  {
    public Guid ReferenceId { get; set; }
    public string Type { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public int Size { get; set; }
    public string StorePath { get; set; }
    public string Description { get; set; }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b>", this.FileName);
    }
  }
}
