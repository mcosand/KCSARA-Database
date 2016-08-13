namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Data.SqlTypes;
  using System.Linq;

  public class TrainingRequired : ModelObject
  {
    public TrainingRequired()
    {
      From = SqlDateTime.MinValue.Value;
      Until = SqlDateTime.MaxValue.Value;
      JustOnce = false;
    }

    [ForeignKey("CourseId")]
    public virtual TrainingCourse Course { get; set; }
    public Guid CourseId { get; set; }

    public WacLevel WacLevel { get; set; }

    public DateTime From { get; set; }
    public DateTime Until { get; set; }

    /// <summary>How long can the member be in the WAC level before this course must be completed?</summary>
    public int GraceMonths { get; set; }

    public bool JustOnce { get; set; }

    public override string GetReportHtml()
    {
      //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
      //{
      //}
      return string.Format("Required <b>{0}</b> {1}for {2} ({3:yyyy-mm-dd}-{4:yyyy-mm-dd}",
        Course.DisplayName,
        JustOnce ? "once " : string.Empty,
        WacLevel,
        From,
        Until);
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
      if (Until <= From)
      {
        yield return new ValidationResult("Must be later than From", new[] { "Until" });
      }
    }
  }
}
