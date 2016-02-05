/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System.IO;

  public class DownloadStream
  {
    public string Filename { get; set; }
    public string MimeType { get; set; }
    public Stream Stream { get; set; }
  }
}
