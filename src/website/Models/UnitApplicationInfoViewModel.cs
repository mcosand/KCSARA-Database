/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kcsar.Database.Model;

namespace Kcsara.Database.Web.Model
{
    public class UnitApplicationInfoViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AppText { get; set; }
        public ApplicationStatus Status { get; set; }
        public string Contact { get; set; }
    }
}
