/*
 * Copyright 2009-2015 Matthew Cosand
 */

namespace Kcsar.Database.Data
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Kcsar.Database.Data.Events;

  [Table("ComputedTrainingRecords")]
  public partial class ComputedTrainingRecordRow : ITrainingAwardRow
  {
    public ComputedTrainingRecordRow()
      : base()
    {
      this.Id = Guid.NewGuid();
    }

    public Guid Id { get; protected set; }

    public DateTime? Expiry { get; set; }
    
    [ForeignKey("CourseId")]
    public virtual TrainingCourseRow Course { get; set; }
    public Guid CourseId { get; set; }
    
    [ForeignKey("RuleId")]
    public virtual TrainingRuleRow Rule { get; set; }
    public Guid? RuleId { get; set; }
    
    [ForeignKey("MemberId")]
    public virtual MemberRow Member { get; set; }
    public virtual Guid MemberId { get; set; }

    public DateTime? Completed { get; set; }

    [ForeignKey("AttendanceId")]
    public virtual ParticipantRow Attendance { get; set; }
    public Guid? AttendanceId { get; set; }


    public ComputedTrainingRecordRow(TrainingRecordRow award)
      : this()
    {
      this.Member = award.Member;
      this.Course = award.Course;
      this.Attendance = award.Attendance;
      this.AttendanceId = award.AttendanceId;
      this.Expiry = award.Expiry;
      this.Completed = award.Completed;
    }

    public DateTime? NullableCompleted { get { return this.Completed; } }

    public override string ToString()
    {
      return string.Format("(0} awarded to {1}, expiring {2}", this.Course.DisplayName, this.Member.FullName, this.Expiry);
    }
  }
}
