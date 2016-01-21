/*
 * Copyright 2015-2016 Matthew Cosand
 */
 namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class ExternalLogin
  {
    [Key]
    [Column(Order = 0)]
    public Guid Id { get; set; }

    [Required]
    public virtual MemberRow Member { get; set; }

    [Key]
    [Column(Order = 1)]
    public string Provider { get; set; }

    [Key]
    [Column(Order = 2)]
    public string Login { get; set; }
  }
}
