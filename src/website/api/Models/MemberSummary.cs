/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api.Models
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Model;

  public class MemberSummary
  {
    public Guid Id { get; set; }
    public string Name { get; set; }

    [Newtonsoft.Json.JsonProperty("DEM")]
    public string WorkerNumber { get; set; }


    public NameIdPair[] Units { get; set; }

    public MemberSummary()
    {
    }

    public MemberSummary(Member member)
    {
      this.Id = member.Id;
      this.Name = member.ReverseName;
      this.WorkerNumber = member.DEM;
    }
  }
}
