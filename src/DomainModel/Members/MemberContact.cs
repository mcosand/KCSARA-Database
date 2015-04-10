/*
 * Copyright 2010-2015 Matthew Cosand
 */

namespace Kcsara.Database.Model.Members
{
  using System;

  public class MemberContact
  {
    public Guid Id { get; set; }

    public string Type { get; set; }

    public string SubType { get; set; }

    public string Value { get; set; }

    public int Priority { get; set; }
  }
}
