/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;

  public class ParticipationSummary
  {
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public string Name { get; set; }
    public Guid EventId { get; set; }
    public string Number { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
