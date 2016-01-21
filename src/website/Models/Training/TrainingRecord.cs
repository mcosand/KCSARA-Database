/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models.Training
{
  using System;

  public class TrainingRecord
  {
    public Guid Id { get; set; }
    public NameIdPair Course { get; set; }
    public NameIdPair Member { get; set; }
    public DateTime Completed { get; set; }
    public DateTime? Expires { get; set; }

    public NameIdPair Event { get; set; }
  }
}
