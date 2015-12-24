/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Collections.Generic;
    using Kcsar.Database.Model;

    public class RosterRowsContext
    {
        public IEnumerable<IRosterEntry> Rows { get; set; }
        public RosterType Type { get; set; }
    }

    public class ExpandedRowsContext
    {
        public IList<IRosterEntry> Rows { get; set; }
        public IEnumerable<IRosterEntry> BadRows { get; set; }
        public DateTime RosterStart { get; set; }
        public int NumDays { get; set; }
        public Guid EventId { get; set; }
        public RosterType Type { get; set; }
        public IRosterEvent SarEvent { get; set; }
    }

    public enum RosterType
    {
        Mission,
        Training
    }
}
