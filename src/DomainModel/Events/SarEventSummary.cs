/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;

  public class SarEventSummary : BaseSarEvent
  {
    public int Participants { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
