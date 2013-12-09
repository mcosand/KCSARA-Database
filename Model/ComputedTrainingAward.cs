
namespace Kcsar.Database.Model
{
    using System;

    public partial class ComputedTrainingAward : ITrainingAward
    {
        public ComputedTrainingAward()
            : base()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; protected set; }

        public DateTime? Expiry { get; set; }
        public virtual TrainingCourse Course { get; set; }
        public virtual TrainingRule Rule { get; set; }
        public virtual Member Member { get; set; }
        public DateTime? Completed { get; set; }
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

        public DateTime? NullableCompleted { get { return this.Completed; } }

        public override string ToString()
        {
            return string.Format("(0} awarded to {1}, expiring {2}", this.Course.DisplayName, this.Member.FullName, this.Expiry);
        }
    }
}