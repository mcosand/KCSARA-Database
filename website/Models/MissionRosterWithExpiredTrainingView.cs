using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MissionRosterWithExpiredTrainingView
    {
        [DataMember]
        public MemberSummaryRow Member { get; set; }

        [DataMember]
        public EventSummaryView Mission { get; set; }

        [DataMember]
        public string[] ExpiredTrainings { get; set; }
    }
}