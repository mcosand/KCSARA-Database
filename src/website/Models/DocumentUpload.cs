/*
 * Copyright 2016 Matthew Cosand
 */
using Microsoft.AspNet.Http;

namespace Kcsara.Database.Web.Models
{
  public class DocumentUpload
  {
    public IFormFile File { get; set; }

    public string Description { get; set; }
    public string Type { get; set; }
  }
}