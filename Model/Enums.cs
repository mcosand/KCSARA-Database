/*
 * Copyright 2008-2014 Matthew Cosand
 */

using System;
using System.ComponentModel.DataAnnotations;
namespace Kcsar.Database.Model
{
  //public enum CardType
  //{
  //    Unknown,
  //    None,
  //    Admin,
  //    Field,
  //    Novice,
  //    Support,
  //    External,
  //    Deputy
  //};

  public enum CloudCover
  {
    Clear,
    Fog,
    Overcast,
    PartlyCloudy,
    Stormy
  }

  public enum RainType
  {
    None,
    Drizzle,
    Occasional,
    Heavy
  }

  public enum SnowType
  {
    None,
    Light,
    Occasional,
    Heavy
  }

  public enum Terrain
  {
    Prairie,
    Rural,
    Suburban,
    Urban,
    Wilderness
  }

  public enum Topography
  {
    Flat,
    Mountain,
    Rolling,
    Rugged
  }

  public enum Density
  {
    None,
    Some,
    Moderate,
    Dense
  }

  public enum Water
  {
    None,
    Canal,
    Lake,
    River,
    Bay,
    Marsh
  }


  public enum Gender
  {
    Unknown,
    Male,
    Female
  }

  /// <summary>Washington Administrative Code (WAC) Worker Types</summary>
  public enum WacLevel
  {
    /// <summary>Not an emergency worker.</summary>
    None = 0,

    // Not currently used. Need to clarify with county about invoking this for
    // those members that support a search mission from in town (comm operators).
    // Wouldn't need to complete WACs, but could be put on a roster, etc.
    ///// <summary>Administrative duties. Not a SAR worker.</summary>
    //Admin = 2,

    /// <summary>SAR Member with less than 1 year experience.</summary>
    Novice = 1,

    /// <summary>SAR Member in a base support capacity.</summary>
    Support = 3,

    /// <summary>Fully qualified Search and Rescue member.</summary>
    Field = 4
  }

  public enum PersonAddressType
  {
    /// <summary>The address at which the person receives their mail, other than their residence.</summary>
    Mailing,

    /// <summary>The location of the person's residence (where they might get picked up for a mission).</summary>
    Residence,

    /// <summary>Other Address.</summary>
    Other
  }

  [Flags]
  public enum MemberStatus
  {
    None = 0,
    Unknown = 1,
    Applicant = 2,
    WaitingBG = 6, // Applicant + 4
    ProcessingBG = 14, // Applicant + 8
    Member = 16,
    Mission = 48, // Member + 32
  }

  [Flags]
  public enum UnitDocumentType
  {
    Application = 1
  }

  public enum DocumentStatus
  {
    NotApplicable = 0,
    NotStarted = 1,
    Submitted = 2,
    UnderReview = 3,
    Complete = 4
  }

  public enum ResponderStatus
  {
    [Display(Name = "")]
    Unknown = 0,
    [Display(Name = "Standing By")]
    StandingBy = 10,
    Responding = 20,
    [Display(Name = "At Base")]
    AtBase = 30,
    [Display(Name = "Returning Home")]
    ReturningHome = 40,
    Cleared = 50,
    [Display(Name = "In Town")]
    InTown = 60
  }

  public enum MissionRole
  {
    [Display(Name = "")]
    Unknown = 0,
    Base = 2,
    Field = 3,
    [Display(Name = "Unit Lead")]
    UnitLead = 4,
    [Display(Name = "In-Town")]
    InTown = 5
  }
}
