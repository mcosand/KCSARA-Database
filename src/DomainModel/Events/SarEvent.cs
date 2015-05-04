/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public abstract class BaseSarEvent
  {
    public Guid Id { get; set; }
    
    [Required]
    public string Title { get; set; }
    
    [Required]
    public DateTime Start { get; set; }
    public string Location { get; set; }
    public string IdNumber { get; set; }

  }

  public abstract class SarEvent : BaseSarEvent
  {
    public string Jurisdiction { get; set; }
    public DateTime? Stop { get; set; }
    public List<string> MissionType { get; set; }
  }

  public class Mission : SarEvent
  {
  }

  public class Training : SarEvent
  {
  }
}
