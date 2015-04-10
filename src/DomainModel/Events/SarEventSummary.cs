/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;

  public class SarEventSummary
  {
    public Guid Id { get; set; }
    public string Number { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public int Participants { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
