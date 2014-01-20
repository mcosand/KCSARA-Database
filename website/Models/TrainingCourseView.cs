/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Runtime.Serialization;
    using System.Linq.Expressions;
    using Kcsar.Database.Model;
    
    [DataContract(Namespace="")]
    public class TrainingCourseView
    {
        [DataMember(EmitDefaultValue=false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Title { get; set; }

        [DataMember]
        public int Required { get; set; }

        [DataMember]
        public bool? Offered { get; set; }

        public static Expression<Func<TrainingCourse, TrainingCourseView>> GetTrainingCourseConversion(DateTime? when)
        {
            return f => new TrainingCourseView
            {
                Id = f.Id,
                Title = f.DisplayName,
                Required = f.WacRequired,
                Offered = (when == null) ? (bool?)null : ((f.OfferedFrom ?? DateTime.MinValue) <= when) && ((f.OfferedUntil ?? DateTime.MaxValue) > when)
            };
        }
    }
}
