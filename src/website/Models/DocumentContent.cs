/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  public class DocumentContent
  {
    public byte[] Data { get; set; }

    public string Filename { get; set; }

    public string Mime { get; set; }
  }
}