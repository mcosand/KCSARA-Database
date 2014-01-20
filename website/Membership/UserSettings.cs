/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Membership
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Web;
    using System.Web.Profile;
    using System.Web.Security;
    using Kcsar.Membership;
    using Kcsar.Database;

    [Serializable]
    public class UserSettings
    {
        public List<Guid> UnitFilter { get; set; }
        public DateTime? AtTime { get; set; }
        public CoordinateDisplay CoordinateDisplay { get; set; }
    }

    public enum CoordinateDisplay
    {
        DecimalDegrees,
        DecimalMinutes,
        DegreesMinutesSeconds
    }
}
