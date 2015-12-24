/*
 * Copyright 2009-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class AccountListRow
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public DateTime? LastActive { get; set; }
        [DataMember]
        public string LinkKey { get; set; }
        [DataMember]
        public string ExternalSources { get; set; }
        [DataMember]
        public bool? IsLocked { get; set; }
        
    }
}
