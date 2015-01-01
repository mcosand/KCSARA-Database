/*
 * Copyright 2011-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract(Name = "NameId")]
    public class NameIdViewModel
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid Parent { get; set; }
    }
}
