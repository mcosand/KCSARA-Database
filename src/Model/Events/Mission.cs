/*
 * Copyright 2015 Matthew Cosand
 */

using System.Collections.Generic;
namespace Kcsar.Database.Model.Events
{
  public class Mission : SarEvent
  {
  }

  public class Training : SarEvent
  {
    public virtual ICollection<TrainingCourse> OfferedCourses { get; set; }
  }
}
