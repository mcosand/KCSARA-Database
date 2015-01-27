/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class AccountSignup : AccountRegistration
    {
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public Guid[] Units { get; set; }
    }
}
