/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    public class AccountVerify
    {
        public string Username { get; set; }
        public string Key { get; set; }
    }
}
