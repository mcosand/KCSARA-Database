using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using Kcsar.Database.Model;

    public class BadgeContext
    {
        public Member Member { get; set; }
        public bool IsPassport { get; set; }
    }
}
