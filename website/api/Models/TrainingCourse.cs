/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Runtime.Serialization;
    using System.Linq.Expressions;
    using Model = Kcsar.Database.Model;
    
    [DataContract(Namespace="")]
    public class TrainingCourse
    {
        [DataMember(EmitDefaultValue=false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Title { get; set; }

        [DataMember]
        public int Required { get; set; }

        [DataMember]
        public bool? Offered { get; set; }

        public static Expression<Func<Model.TrainingCourse, TrainingCourse>> GetTrainingCourseConversion(DateTime? when)
        {
            return f => new TrainingCourse
            {
                Id = f.Id,
                Title = f.DisplayName,
                Required = f.WacRequired,
                Offered = (when == null) ? (bool?)null : ((f.OfferedFrom ?? DateTime.MinValue) <= when) && ((f.OfferedUntil ?? DateTime.MaxValue) > when)
            };
        }
    }
}
