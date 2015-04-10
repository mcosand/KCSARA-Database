/*
 * Copyright 2010-2015 Matthew Cosand
 */

namespace Kcsara.Database.Model.Members
{
  using System;

  public class MemberAddress
  {
    public Guid Id { get; set; }

    public string Type { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }
    public string Zip { get; set; }
  }
}
