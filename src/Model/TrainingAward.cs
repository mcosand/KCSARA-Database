/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Threading;
  using Kcsar.Database.Model.Events;

  public class TrainingAward : ModelObject, ITrainingAward
  {
    [Required]
    public DateTime Completed { get; set; }
    public DateTime? Expiry { get; set; }
    public string DocPath { get; set; }
    public string metadata { get; set; }
    [Required]
    public virtual Member Member { get; set; }
    [Required]
    public virtual TrainingCourse Course { get; set; }

    [ForeignKey("AttendanceId")]
    public virtual Participant Attendance { get; set; }
    public Guid? AttendanceId { get; set; }

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
      return string.Format("<b>{0}</b> awarded <b>{1}</b>, {2:d}. Expires:{3:d}, Have Roster={4}", this.Member.FullName, this.Course.DisplayName, this.Completed, this.Expiry, this.AttendanceId != null);
    }

    public override string ToString()
    {
      return this.GetReportHtml();
    }


    public DateTime? NullableCompleted { get { return new DateTime?(this.Completed); } }

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
