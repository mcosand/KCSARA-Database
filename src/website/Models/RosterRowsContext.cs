/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Data.Events;
  using Kcsar.Database.Model;
  
  public class RosterRowsContext
  {
    public IEnumerable<EventRosterRow> Rows { get; set; }
    public RosterType Type { get; set; }
  }

  public class ExpandedRowsContext
  {
    public IList<EventRosterRow> Rows { get; set; }
    public IEnumerable<EventRosterRow> BadRows { get; set; }
    public DateTime RosterStart { get; set; }
    public int NumDays { get; set; }
    public Guid EventId { get; set; }
    public RosterType Type { get; set; }
    public SarEventRow SarEvent { get; set; }
  }

  public enum RosterType
  {
    Mission,
    Training
  }
}
