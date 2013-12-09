using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class MembersApplication
    {
        public Guid Id { get; set; }
        public NameIdPair Unit { get; set; }
        public DateTime Started { get; set; }
        public bool IsActive { get; set; }
        public bool CanEdit { get; set; }
    }
}