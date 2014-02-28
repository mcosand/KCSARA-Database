/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Threading;

  [MemberReporting("WacLevelDate", Format = "{0:yyyy-MM-dd}")]
  public class Member : ModelObject
  {
    [MaxLength(50)]
    public string DEM { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public DateTime? InternalBirthDate { get; set; }
    public string InternalGender { get; set; }
    public string PhotoFile { get; set; }
    public int InternalWacLevel { get; set; }
    public DateTime WacLevelDate { get; set; }
    public string Username { get; set; }
    public string Comments { get; set; }
    public DateTime? BackgroundDate { get; set; }
    public DateTime? SheriffApp { get; set; }
    public int? ExternalKey1 { get; set; }
    public MemberStatus Status { get; set; }
    public virtual MemberMedical MedicalInfo { get; set; }

    public virtual ICollection<MissionLog> MissionLogs { get; set; }
    public virtual ICollection<MissionRoster> MissionRosters { get; set; }
    public virtual ICollection<PersonAddress> Addresses { get; set; }
    public virtual ICollection<PersonContact> ContactNumbers { get; set; }
    public virtual ICollection<TrainingAward> TrainingAwards { get; set; }
    public virtual ICollection<TrainingRoster> TrainingRosters { get; set; }
    public virtual ICollection<UnitMembership> Memberships { get; set; }
    public virtual ICollection<ComputedTrainingAward> ComputedAwards { get; set; }
    public virtual ICollection<AnimalOwner> Animals { get; set; }
    public virtual ICollection<MissionDetails> MissionDetails { get; set; }
    public virtual ICollection<UnitApplicant> ApplyingTo { get; set; }
    public virtual ICollection<MemberEmergencyContact> EmergencyContacts { get; set; }
    public virtual ICollection<MemberUnitDocument> UnitDocuments { get; set; }

    public Member()
      : base()
    {
      this.WacLevelDate = DateTime.Today;
      this.LastChanged = DateTime.Now;
      this.ChangedBy = Thread.CurrentPrincipal.Identity.Name;

      this.MissionLogs = new List<MissionLog>();
      this.MissionRosters = new List<MissionRoster>();
      this.Addresses = new List<PersonAddress>();
      this.ContactNumbers = new List<PersonContact>();
    }

    public override string ToString()
    {
      return this.ReverseName;
    }

    [NotMapped]
    public string FullName
    {
      get { return string.Format("{0} {1}", this.FirstName, this.LastName); }
    }

    [NotMapped]
    public string ReverseName
    {
      get { return string.Format("{0}, {1}", this.LastName, this.FirstName); }
      set { }
    }

    [NotMapped]
    public string BackgroundText
    {
      get
      {
        return ((this.Status & MemberStatus.ProcessingBG) == MemberStatus.ProcessingBG) ? "Processing"
            : ((this.Status & MemberStatus.WaitingBG) == MemberStatus.WaitingBG) ? "Requested"
            : (this.BackgroundDate == null) ? "Not Recorded"
            : this.BackgroundDate.Value.ToString("yyyy-MM-dd");
      }
    }

    public UnitMembership[] GetActiveUnits()
    {
      return GetActiveUnits(DateTime.Now);
    }

    public UnitMembership[] GetActiveUnits(DateTime atTime)
    {
      return (from um in this.Memberships where um.EndTime == null && um.Status != null && um.Status.IsActive == true select um).ToArray();
    }

    public bool? IsYouth
    {
      get
      {
        if (this.InternalBirthDate.HasValue)
        {
          return this.InternalBirthDate.Value.Date.AddYears(18) >= DateTime.Now.Date;
        }
        return null;
      }
    }

    [Reporting(Hides = "InternalGender")]
    [NotMapped]
    public Gender Gender
    {
      get
      {
        Gender result = Gender.Unknown;
        foreach (string s in Enum.GetNames(typeof(Gender)))
        {
          if (string.Equals(this.InternalGender, s.Substring(0, 1), StringComparison.OrdinalIgnoreCase))
          {
            result = (Gender)Enum.Parse(typeof(Gender), s, true);
          }
        }
        return result;
      }

      set
      {
        if (value == Gender.Unknown)
        {
          this.InternalGender = null;
        }
        else
        {
          this.InternalGender = value.ToString().Substring(0, 1).ToLower();
        }
      }
    }


    /// <summary>Changing this value will also change WacLevelDate to the current date. Manually set WacLevelDate afterwards if necessary.</summary>
    [DataMember]
    [Reporting(Hides = "InternalWacLevel")]
    [NotMapped]
    public WacLevel WacLevel
    {
      get
      {
        return (WacLevel)this.InternalWacLevel;
      }

      set
      {
        if (value != this.WacLevel)
        {
          this.InternalWacLevel = (int)value;
          this.WacLevelDate = DateTime.Today;
        }
      }
    }

    [Reporting(Hides = "InternalBirthDate", Format = "{0:yyyy-MM-dd}")]
    [NotMapped]
    public DateTime? BirthDate
    {
      get
      {
        return this.InternalBirthDate;
      }

      set
      {
        this.InternalBirthDate = value;
      }
    }

    private int GetTrainingFilter(int offset)
    {
      int multiplier = (int)this.WacLevel - 1;
      return 1 << (2 * multiplier + offset);
    }

    public int RequiredTrainingFilter
    {
      get
      {
        return GetTrainingFilter(1);
      }
    }

    public bool IsTrainingRequired(TrainingCourse course)
    {
      return (course.WacRequired & this.RequiredTrainingFilter) > 0;
    }

    public bool ShouldKeepCourseCurrent(TrainingCourse course)
    {
      return this.IsTrainingRequired(course) && ((course.WacRequired & GetTrainingFilter(0)) > 0);
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> DEM:{1}, {2}, <span class=\"personal\">DOB:{3}, </span>WAC:{4}", this.FullName, this.DEM, this.Gender, this.BirthDate, this.WacLevel);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (string.IsNullOrEmpty(this.FirstName))
      {
        yield return new ValidationResult("Required", new[] { "FirstName" });
      }
    }
  }
}
