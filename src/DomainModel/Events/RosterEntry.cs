/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Model.Events
{
  using System;

  public class RosterEntry
  {
    public Guid Id { get; set; }
    public SarEvent Event { get; set; }
    public Participant Participant { get; set; }
    public NameIdPair Unit { get; set; }
    public double? Hours { get; set; }
    public int? Miles { get; set; }
  }
}
