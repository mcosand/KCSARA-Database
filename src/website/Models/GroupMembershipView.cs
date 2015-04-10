/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Model
{
  using System.Runtime.Serialization;
  using System.Web.Mvc;
  using Kcsara.Database.Model.Members;
  using Kcsara.Database.Web.api.Models;

  public class GroupMembershipView
  {
    public string GroupName { get; set; }
    public bool CanEdit { get; set; }

    public bool ShowGroups { get; set; }
    public SelectList CurrentGroups { get; set; }
    public SelectList OtherGroups { get; set; }

    public SelectList CurrentUsers { get; set; }
    public SelectList OtherUsers { get; set; }
  }

  [DataContract]
  public class GroupView
  {
    [DataMember]
    public string Name { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public MemberSummary[] Owners { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string[] Destinations { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string EmailAddress { get; set; }

    [DataMember(EmitDefaultValue = false)]
    public string[] SubscribedAddresses { get; set; }
  }
}
