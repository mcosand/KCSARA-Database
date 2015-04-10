/*
 * Copyright 2010-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Model
{
  using System;
  using System.Runtime.Serialization;
  using Kcsara.Database.Model.Members;
  using Kcsara.Database.Web.api.Models;

  [DataContract]
  public class MemberRosterRow
  {
    [DataMember]
    public MemberSummary Person { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public double? Hours { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public int Count { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public int Miles { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public DateTime? Date { get; set; }
  }
}
