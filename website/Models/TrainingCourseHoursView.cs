/*
 * Copyright 2010-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    public class TrainingCourseHoursView
    {
        public string CourseName { get; set; }
        public Guid CourseId { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
    }
}
