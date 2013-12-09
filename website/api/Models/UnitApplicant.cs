using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class UnitApplicant
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public string NameReverse { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int EmergencyContactCount { get; set; }
        public int RemainingDocCount { get; set; }

        public string Background { get; set; }
        public DateTime Started { get; set; }
        public bool Active { get; set; }
    }
}