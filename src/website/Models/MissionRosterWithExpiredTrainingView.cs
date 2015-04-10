/*
 * Copyright 2011-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Model
{
  using System.Runtime.Serialization;
  using Kcsara.Database.Model.Members;
  using Kcsara.Database.Web.api.Models;

  [DataContract]
  public class MissionRosterWithExpiredTrainingView
  {
    [DataMember]
    public MemberSummary Member { get; set; }

    [DataMember]
    public EventSummaryView Mission { get; set; }

    [DataMember]
    public string[] ExpiredTrainings { get; set; }
  }
}
