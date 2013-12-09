using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class MemberDetailView
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public MemberContactView[] Contacts { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string[] Units { get; set; }
    }
}
