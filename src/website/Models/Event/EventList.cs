/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;
  using System.Collections.Generic;

  public class EventList
  {
    public IEnumerable<EventListItem> Events { get; set; }
    public int People { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }

  public class EventListItem
  {
    public Guid Id { get; set; }
    public string Number { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; }
    public int People { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}