/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  public class EventStatistics
  {
    public EventStatisticsItem YearToDate { get; set; }
    public EventStatisticsItem Recent { get; set; }
    public EventStatisticsItem Average { get; set; }
  }

  public class EventStatisticsItem
  {
    public int Count { get; set; }
    public int People { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
