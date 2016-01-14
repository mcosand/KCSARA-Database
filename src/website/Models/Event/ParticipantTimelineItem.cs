/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Model;
  public class ParticipantTimelineItem
  {
    public ParticipantTimelineItem()
    {
      
    }

    public Guid Id { get; set; }

    public DateTime Time { get; set; }

    public int? Miles { get; set; }

    public ParticipatingNameIdPair Unit { get; set; }

    public ParticipantStatus Status { get; set; }

    public string Role { get; set; }
  }
}