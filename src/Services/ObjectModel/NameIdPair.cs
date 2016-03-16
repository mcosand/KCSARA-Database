/*
 * Copyright 2016 Matthew Cosand
 */
using System;

namespace Kcsara.Database.Model
{
  public class NameIdPair
  {
    public NameIdPair() { }
    public NameIdPair(Guid id, string name) : this()
    {
      Id = id;
      Name = name;
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
  }
}
