/*
 * Copyright 2010-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class LogSubmission
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid MissionId { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime? Time { get; set; }

        [DataMember]
        public MemberView Person { get; set; }
    }

    [DataContract]
    public class MemberView
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Card { get; set; }
    }
}
