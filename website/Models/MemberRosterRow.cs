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
    public class MemberRosterRow
    {
        [DataMember]
        public MemberSummaryRow Person { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double? Hours { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Count { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Miles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? Date { get; set; }
    }
}
