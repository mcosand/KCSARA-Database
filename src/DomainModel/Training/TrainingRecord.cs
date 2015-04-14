/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Training
{
  using System;
  using System.ComponentModel.DataAnnotations;

  public class TrainingRecord
  {
    public Guid Id { get; set; }
    [Required]
    public DateTime Completed { get; set; }
    public DateTime? Expiry { get; set; }

    public NameIdPair Member { get; set; }
    public NameIdPair Course { get; set; }
    public NameIdPair Attendance { get; set; }
    public Guid? RuleId { get; set; }
  }
}
