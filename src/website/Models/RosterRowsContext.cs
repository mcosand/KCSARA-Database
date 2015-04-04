/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Collections.Generic;
    using Kcsar.Database.Model;
  using Kcsar.Database.Model.Events;

    public class RosterRowsContext
    {
        public IEnumerable<EventRoster> Rows { get; set; }
        public RosterType Type { get; set; }
    }

    public class ExpandedRowsContext
    {
      public IList<EventRoster> Rows { get; set; }
      public IEnumerable<EventRoster> BadRows { get; set; }
        public DateTime RosterStart { get; set; }
        public int NumDays { get; set; }
        public Guid EventId { get; set; }
        public RosterType Type { get; set; }
        public SarEvent SarEvent { get; set; }
    }

    public enum RosterType
    {
        Mission,
        Training
    }
}
