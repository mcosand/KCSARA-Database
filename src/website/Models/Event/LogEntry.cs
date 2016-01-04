/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;
  using System.Collections.Generic;

  public class LogEntry
  {
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public string Message { get; set; }
    public string LoggedBy { get; set; }
  }
}