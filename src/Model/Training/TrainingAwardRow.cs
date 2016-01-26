/*
 * Copyright 2009-2016 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Threading;
  using Events;

  [Table("TrainingAwards")]
  public class TrainingAwardRow : ModelObject, ITrainingAward
  {
    [Required]
    public DateTime Completed { get; set; }
    public DateTime? Expiry { get; set; }
    public string DocPath { get; set; }
    public string metadata { get; set; }
    [Required]
    public virtual MemberRow Member { get; set; }
    [Required]
    public virtual TrainingCourse Course { get; set; }
    public Guid? RosterId { get; set; }
    [ForeignKey("RosterId")]
    public virtual EventParticipantRow RosterEntry { get; set; }


    [NotMapped]
    public bool UploadsPending { get; set; }

    public TrainingAwardRow()
      : base()
    {
      this.LastChanged = DateTime.Now;
      this.ChangedBy = Thread.CurrentPrincipal.Identity.Name;
      this.UploadsPending = false;
      this.Completed = DateTime.Today;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> awarded <b>{1}</b>, {2:d}. Expires:{3:d}, Have Roster={4}", this.Member.FullName, this.Course.DisplayName, this.Completed, this.Expiry, this.RosterEntry != null);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }


    public TrainingRule Rule { get { return null; } }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (this.Completed > DateTime.Today.AddMonths(3))
      {
        yield return new ValidationResult("Too far in the future. Must be less than 3 months from today.", new[] { "Completed" });
      }

      if (this.Expiry.HasValue && this.Completed > this.Expiry)
      {
        yield return new ValidationResult("Expiration must be after time completed.", new[] { "Expiry" });
      }
    }
  }
}
