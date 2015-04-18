/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;

  public abstract class SarEvent
  {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public string IdNumber { get; set; }
  }

  public class Mission : SarEvent
  {
  }

  public class Training : SarEvent
  {
  }
}
