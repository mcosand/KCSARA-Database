/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("SensitiveInfoAccesses")]
  public class SensitiveInfoAccessRow
  {
    public SensitiveInfoAccessRow()
    {
      this.Id = Guid.NewGuid();
    }

    [Key]
    public Guid Id { get; set; }
    public string Actor { get; set; }
    public DateTime Timestamp { get; set; }
    
    [ForeignKey("OwnerId")]
    public virtual MemberRow Owner { get; set; }
    public Guid OwnerId { get; set; }

    public string Action { get; set; }
    public string Reason { get; set; }
  }
}
