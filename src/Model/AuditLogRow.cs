/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("AuditLogs")]
  public class AuditLogRow
  {
    public AuditLogRow()
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

    public AuditLogRow GetCopy()
    {
      return (AuditLogRow)this.MemberwiseClone();
    }
  }
}
