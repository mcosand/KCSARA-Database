/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace="")]
    public class MemberContactView
    {
        [DataMember]
        public Guid Id { get; set; }
        
        [DataMember(EmitDefaultValue=false)]
        public Guid MemberId { get; set; }
        
        [DataMember(EmitDefaultValue=false)]
        public string Type { get; set; }
        
        [DataMember(EmitDefaultValue=false)]
        public string SubType { get; set; }
        
        [DataMember]
        public string Value { get; set; }
        
        [DataMember]
        public int Priority { get; set; }
    }
}
