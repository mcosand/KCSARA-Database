/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Model.Events
{
  using System;

  public class TimelineEntry
  {
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public string Markdown { get; set; }
    public string Type { get; set; }
  }
}
