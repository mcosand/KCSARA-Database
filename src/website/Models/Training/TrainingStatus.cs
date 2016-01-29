/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models.Training
{
  using System;
  using System.Collections.Generic;

  public class TrainingStatus
  {
    public DateTime? Expires { get; set; }
    public ExpirationFlags Status { get; set; }
    public DateTime? Completed { get; set; }
    public NameIdPair Course { get; set; }
  }

  public class CompositeTrainingStatus : TrainingStatus
  {
    public CompositeTrainingStatus()
    {
      Parts = new List<TrainingStatus>();
    }

    public List<TrainingStatus> Parts { get; set; }
  }

  public class ScopedTrainingStatus
  {
    public ScopedTrainingStatus(string name)
    {
      Name = name;
      Courses = new List<TrainingStatus>();
    }

    public string Name { get; private set; }
    public List<TrainingStatus> Courses { get; private set; }
  }

  [Flags]
  public enum ExpirationFlags
  {
    Unknown = 0,
    Okay = 1,
    NotNeeded = 5, // 4 + 1
    Complete = 9, // 8 + 1
    NotExpired = 17, // 16 + 1
    Expired = 32,
    Missing = 64,
  }
}
