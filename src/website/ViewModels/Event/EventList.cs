/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.ViewModels
{
  using System.Collections.Generic;

  public class EventList
  {
    public IEnumerable<object> Events { get; set; }
    public int People { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}