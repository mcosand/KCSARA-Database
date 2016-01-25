/*
 * Copyright 2008-2016 Matthew Cosand
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
  using Events;

  [Table("Members")]
  public class MemberRow : ModelObject
  {
    [MaxLength(50)]
    public string DEM { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string PhotoFile { get; set; }
    public WacLevel WacLevel { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime WacLevelDate { get; set; }
    public string Username { get; set; }
    public virtual ICollection<ExternalLogin> ExternalLogins { get; set; }

    public string Comments { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? BackgroundDate { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? SheriffApp { get; set; }
    public int? ExternalKey1 { get; set; }
    public MemberStatus Status { get; set; }
    public virtual MemberMedicalRow MedicalInfo { get; set; }

    public virtual ICollection<EventParticipantRow> Events { get; set; }
    public virtual ICollection<EventLogRow> MissionLogs { get; set; }
    public virtual ICollection<MissionRoster_Old> MissionRosters { get; set; }
    public virtual ICollection<PersonAddress> Addresses { get; set; }
    public virtual ICollection<PersonContact> ContactNumbers { get; set; }
    public virtual ICollection<TrainingAwardRow> TrainingAwards { get; set; }
    public virtual ICollection<TrainingRoster_Old> TrainingRosters { get; set; }
    public virtual ICollection<UnitMembership> Memberships { get; set; }
    public virtual ICollection<ComputedTrainingAwardRow> ComputedAwards { get; set; }
    public virtual ICollection<AnimalOwner> Animals { get; set; }
    public virtual ICollection<MissionDetails> MissionDetails { get; set; }
    public virtual ICollection<UnitApplicant> ApplyingTo { get; set; }
    public virtual ICollection<MemberEmergencyContactRow> EmergencyContacts { get; set; }
    public virtual ICollection<MemberUnitDocument> UnitDocuments { get; set; }

    public MemberRow()
      : base()
    {
      WacLevelDate = DateTime.Today;
      LastChanged = DateTime.Now;
      ChangedBy = Thread.CurrentPrincipal.Identity.Name;

      MissionLogs = new List<EventLogRow>();
      MissionRosters = new List<MissionRoster_Old>();
      Events = new List<EventParticipantRow>();
      Addresses = new List<PersonAddress>();
      ContactNumbers = new List<PersonContact>();
      ExternalLogins = new List<ExternalLogin>();
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
        if (this.BirthDate.HasValue)
        {
          return this.BirthDate.Value.Date.AddYears(18) >= DateTime.Now.Date;
        }
        return null;
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
