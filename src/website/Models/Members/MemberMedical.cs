/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System.Collections.Generic;

namespace Kcsara.Database.Web.Models
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
