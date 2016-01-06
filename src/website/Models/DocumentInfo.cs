/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;
  using System.Collections.Generic;

  public class DocumentInfo
  {
    public Guid Id { get; set; }
    
    public string Type { get; set; }

    public string Filename { get; set; }
    public string Mime { get; set; }
    public int Size { get; set; }

    public string Description { get; set; }
  }
}