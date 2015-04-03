/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;
  using Kcsar.Database.Model.Events;
    
    public interface ITrainingAward
    {
        Guid Id { get; }
        Member Member { get; }
        DateTime? NullableCompleted { get; }
        DateTime? Expiry { get; }
        TrainingCourse Course { get; }
        TrainingRule Rule { get; }
        Participant Attendance { get; }
        Guid? AttendanceId { get; }
    }
}
