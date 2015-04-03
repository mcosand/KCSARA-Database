/*
 * Copyright 2015 Matthew Cosand
 */

using System.Collections.Generic;
namespace Kcsar.Database.Model.Events
{
  public class Mission2 : SarEvent
  {
  }

  public class Training2 : SarEvent
  {
    public virtual ICollection<TrainingCourse> OfferedCourses { get; set; }
  }
}
