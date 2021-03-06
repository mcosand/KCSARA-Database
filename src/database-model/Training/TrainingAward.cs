﻿namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Threading;

  public class TrainingAward : ModelObject, ITrainingAward
  {
    public long D4HId { get; set; }

    [Required]
    public DateTimeOffset Completed { get; set; }
    public DateTimeOffset? Expiry { get; set; }
    public string DocPath { get; set; }
    public string metadata { get; set; }
    
    [ForeignKey("MemberId")]
    public virtual Member Member { get; set; }

    [Column("Member_Id")]
    public Guid MemberId { get; set; }

    [Required]
    public virtual TrainingCourse Course { get; set; }
    public virtual TrainingRoster Roster { get; set; }


    [NotMapped]
    public bool UploadsPending { get; set; }

    public TrainingAward()
      : base()
    {
      this.LastChanged = DateTime.Now;
      this.ChangedBy = Thread.CurrentPrincipal.Identity.Name;
      this.UploadsPending = false;
      this.Completed = DateTime.Today;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> awarded <b>{1}</b>, {2:d}. Expires:{3:d}, Have Roster={4}", this.Member.FullName, this.Course.DisplayName, this.Completed, this.Expiry, this.Roster != null);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }


    public DateTimeOffset? NullableCompleted { get { return new DateTimeOffset?(this.Completed); } }

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
