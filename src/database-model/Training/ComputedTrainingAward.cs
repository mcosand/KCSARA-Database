﻿namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  public partial class ComputedTrainingAward : ITrainingAward
  {
    public ComputedTrainingAward()
        : base()
    {
      this.Id = Guid.NewGuid();
    }

    public Guid Id { get; protected set; }

    public DateTimeOffset? Expiry { get; set; }
    [Column("Course_Id")]
    public Guid? CourseId { get; set; }
    [ForeignKey("CourseId")]
    public virtual TrainingCourse Course { get; set; }

    [Column("Rule_Id")]
    public Guid? RuleId { get; set; }
    [ForeignKey("RuleId")]
    public virtual TrainingRule Rule { get; set; }

    public virtual Member Member { get; set; }
    public DateTimeOffset? Completed { get; set; }
    public virtual TrainingRoster Roster { get; set; }

    public ComputedTrainingAward(TrainingAward award)
        : this()
    {
      this.Member = award.Member;
      this.Course = award.Course;
      this.Roster = award.Roster;
      this.Expiry = award.Expiry;
      this.Completed = award.Completed;
    }

    public DateTimeOffset? NullableCompleted { get { return this.Completed; } }

    public override string ToString()
    {
      return string.Format("(0} awarded to {1}, expiring {2}", this.Course.DisplayName, this.Member.FullName, this.Expiry);
    }
  }
}
