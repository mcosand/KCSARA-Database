using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TrainingAwardView
    {
        [DataMember(EmitDefaultValue = false)]
        public MemberSummaryRow Member { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public TrainingCourseView Course { get; set; }

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
    public class TrainingExpirationView : TrainingAwardView
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string ExpiryText { get; set; }
    }

    [DataContract]
    public class CompositeExpirationView
    {
        [DataMember]
        public bool? Goodness { get; set; }

        [DataMember]
        public TrainingExpirationView[] Expirations { get; set; }
    }
}