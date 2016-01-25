/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System;

namespace Kcsara.Database.Web.Models
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
