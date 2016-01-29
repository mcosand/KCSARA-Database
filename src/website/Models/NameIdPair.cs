/*
 * Copyright 2013-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;

  public class NameIdPair
  {
    public NameIdPair() { }
    public NameIdPair(Guid id, string name)
    {
      Id = id;
      Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
  }

  public class ParticipatingNameIdPair : NameIdPair
  {
    public Guid? PermanentId { get; set; }
  }
}
