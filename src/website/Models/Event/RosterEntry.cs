/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Models
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Model;
  public class Roster
  {
    public Roster()
    {
      Responders = new List<RosterEntry>();
    }
    public IEnumerable<RosterEntry> Responders { get; set; }
    public int ResponderCount { get; set; }
  }

  public class RosterEntry
  {
    public Guid Id { get; set; }

    public Guid? MemberId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public NameIdPair Unit { get; set; }

    public bool HasTimeline { get; set; }

    public double? Hours { get; set; }

    public int? Miles { get; set; }

    public string Photo { get; set; }

    public ParticipantStatus Status { get; set; }
  }
}