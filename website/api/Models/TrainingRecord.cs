/*
 * Copyright 2010-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TrainingRecord
    {
        [DataMember(EmitDefaultValue = false)]
        public MemberSummary Member { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public TrainingCourse Course { get; set; }

        [DataMember]
        public string Completed { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Expires { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string ExpirySrc { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Source { get; set; }

        [DataMember]
        public Guid ReferenceId { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public string Comments { get; set; }

        [DataMember]
        public bool? Required { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public int PendingUploads { get; set; }
    }

    [DataContract]
    public class TrainingExpiration : TrainingRecord
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string ExpiryText { get; set; }
    }

    [DataContract]
    public class CompositeExpiration
    {
        [DataMember]
        public bool? Goodness { get; set; }

        [DataMember]
        public TrainingExpiration[] Expirations { get; set; }
    }
}
