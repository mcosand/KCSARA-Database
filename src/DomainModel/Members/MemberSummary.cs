/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Model.Members
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Model;

  public class MemberSummary
  {
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string IdNumber { get; set; }
    public bool HasPhoto { get; set; }
    public WacLevel CardType { get; set; }

    public Dictionary<Guid, string> Units { get; set; }
  }
}
