/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class MemberMedical
    {
        public NameIdPair Member { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string Disclosure { get; set; }
        public IEnumerable<EmergencyContact> Contacts { get; set; }

        public bool IsSensitive { get; set; }
    }
}
