
namespace Kcsar.Database.Model
{
    using System;
    
    public interface ITrainingAward
    {
        Guid Id { get; }
        Member Member { get; }
        DateTime? NullableCompleted { get; }
        DateTime? Expiry { get; }
        TrainingCourse Course { get; }
        TrainingRule Rule { get; }
        TrainingRoster Roster { get; }
    }
}
