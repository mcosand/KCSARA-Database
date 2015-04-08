/*
 * Copyright 2010-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;

  public interface IDocumentRow
  {
    Guid Id { get; }
    string Type { get; set; }
    Guid ReferenceId { get; set; }
    string FileName { get; set; }
    string MimeType { get; set; }
  }
}
