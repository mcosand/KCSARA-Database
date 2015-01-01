/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Runtime.Serialization;
    using Kcsar.Database.Model;

    [DataContract]
    public class MemberSummary
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        
        [DataMember(Name="DEM")]
        public string WorkerNumber { get; set; }
        

        [IgnoreDataMember]
        public Dictionary<Guid, string> Units { get; set; }

        public MemberSummary()
        {
        }

        public MemberSummary(Member member)
        {
            this.Id = member.Id;
            this.Name = member.ReverseName;
            this.WorkerNumber = member.DEM;
        }
    }
}
