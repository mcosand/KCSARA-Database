namespace Kcsar.Database.Model
{
    using System;
    
    public interface ITrainingAward
    {
        Guid Id { get; }
        Member Member { get; }
        DateTimeOffset? NullableCompleted { get; }
        DateTimeOffset? Expiry { get; }
        TrainingCourse Course { get; }
        TrainingRule Rule { get; }
        TrainingRoster Roster { get; }
    }
}
