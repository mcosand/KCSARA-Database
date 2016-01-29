/*
 * Copyright 2009-2016 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Events;
  public interface ITrainingAward
  {
    Guid Id { get; }
    MemberRow Member { get; }
    DateTime Completed { get; }
    DateTime? Expiry { get; }
    TrainingCourse Course { get; }

    TrainingRule Rule { get; }

    Guid? RosterId { get; set; }
    EventParticipantRow RosterEntry { get; set; }
  }
}
