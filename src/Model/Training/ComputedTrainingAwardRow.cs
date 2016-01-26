/*
 * Copyright 2009-2016 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Events;

  [Table("ComputedTrainingAwards")]
  public partial class ComputedTrainingAwardRow : ITrainingAward
  {
    public ComputedTrainingAwardRow()
        : base()
    {
      this.Id = Guid.NewGuid();
    }

    public Guid Id { get; protected set; }

    public DateTime? Expiry { get; set; }
    public virtual TrainingCourse Course { get; set; }
    public virtual TrainingRule Rule { get; set; }
    public virtual MemberRow Member { get; set; }
    public DateTime Completed { get; set; }

    public Guid? RosterId { get; set; }
    [ForeignKey("RosterId")]
    public virtual EventParticipantRow RosterEntry { get; set; }

    public ComputedTrainingAwardRow(TrainingAwardRow award)
        : this()
    {
      this.Member = award.Member;
      this.Course = award.Course;
      this.RosterEntry = award.RosterEntry;
      this.Expiry = award.Expiry;
      this.Completed = award.Completed;
    }

    public override string ToString()
    {
      return string.Format("(0} awarded to {1}, expiring {2}", this.Course.DisplayName, this.Member.FullName, this.Expiry);
    }
  }
}
