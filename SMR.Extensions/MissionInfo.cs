/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;

  internal class MissionInfo
  {
    public Guid Id { get; set; }
    public string Location { get; set; }
    public string MissionType { get; set; }
    public DateTime StartTime { get; set; }
    public string StateNumber { get; set; }
    public string Title { get; set; }
  }
}