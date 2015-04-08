/*
 * Copyright 2010-2015 Matthew Cosand
 */

namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("Documents")]
  public class DocumentRow : ModelObjectRow, IDocumentRow
  {
    public static string StorageRoot { get; set; }
    public const int StorageTreeDepth = 2;
    public const int StorageTreeSpan = 100;

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

    [NotMapped]
    public byte[] Contents
    {
      get
      {
        if (this._contents == null && !string.IsNullOrWhiteSpace(this.StorePath))
        {
          this._contents = System.IO.File.ReadAllBytes(DocumentRow.StorageRoot + this.StorePath);
        }
        return this._contents;
      }
      set
      {
        this._contents = value;
        // If null, we'll set to empty string. KcsarContext takes care of setting this value when persisted to data store.
        this.StorePath = this.StorePath ?? string.Empty;
      }
    }
    private byte[] _contents = null;
  }
}
