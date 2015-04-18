/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Model.Events
{
  using System;

  public class Participant
  {
    public Guid Id { get; set; }
    public Guid? MemberId { get; set; }
    public string Name { get; set; }
    public string IdNumber { get; set; }
    public NameIdPair Event { get; set; }
  }
}
