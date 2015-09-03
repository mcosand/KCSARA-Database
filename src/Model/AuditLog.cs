/*
 * Copyright 2009-2014 Matthew Cosand
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace Kcsar.Database.Model
{
  public class AuditLog
    {
        public AuditLog()
        {
            this.Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
        public string User { get; set; }
        public DateTime Changed { get; set; }
        public string Collection { get; set; }

        public AuditLog GetCopy()
        {
            return (AuditLog)this.MemberwiseClone();
        }
    }
}
