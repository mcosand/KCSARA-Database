﻿/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("EventDetails")]
  public class EventDetailRow : ModelObjectRow
  {
    public string Clouds { get; set; }
    public double? TempLow { get; set; }
    public double? TempHigh { get; set; }
    public double? WindLow { get; set; }
    public double? WindHigh { get; set; }
    public double? Visibility { get; set; }
    public int? RainType { get; set; }
    public double? RainInches { get; set; }
    public int? SnowType { get; set; }
    public double? SnowInches { get; set; }
    public int? Terrain { get; set; }
    public int? GroundCoverDensity { get; set; }
    public int? GroundCoverHeight { get; set; }
    public int? WaterType { get; set; }
    public int? TimberType { get; set; }
    public int? ElevationLow { get; set; }
    public int? ElevationHigh { get; set; }
    public string Tactics { get; set; }
    public string CluesMethod { get; set; }
    public string TerminatedReason { get; set; }
    public bool? Debrief { get; set; }
    public bool? Cisd { get; set; }
    public string Comments { get; set; }
    public string EquipmentNotes { get; set; }
    public int? Topography { get; set; }

    // ForeignKey not needed - this is a 0-1 multiple and .Event points to SarEvents.Id
    public virtual SarEventRow Event { get; set; }

    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public Guid? MemberId { get; set; }

    public override string ToString()
    {
      return string.Format("Details");
    }

    public override string GetReportHtml()
    {
      return string.Format("Details of [<b>{0}</b>]", this.Event.Title);
    }
  }
}