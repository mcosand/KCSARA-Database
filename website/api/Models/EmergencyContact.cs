using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class EmergencyContact
    {
        public Guid Id { get; set; }
        public bool IsSensitive { get; set; }

        public string Name { get; set; }
        public string Relation { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }

    }
}