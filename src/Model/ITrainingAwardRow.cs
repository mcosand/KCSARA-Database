/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using Kcsar.Database.Data.Events;

  public interface ITrainingAwardRow
  {
    Guid Id { get; }
    MemberRow Member { get; }
    Guid MemberId { get; }

    DateTime? NullableCompleted { get; }
    DateTime? Expiry { get; }
    TrainingCourseRow Course { get; }
    Guid CourseId { get; }

    TrainingRuleRow Rule { get; }
    Guid? RuleId { get; }

    ParticipantRow Attendance { get; }
    Guid? AttendanceId { get; }
  }
}
