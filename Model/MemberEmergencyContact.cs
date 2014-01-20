/*
 * Copyright 2013-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
    public class MemberEmergencyContact : ModelObject
    {
        public virtual Member Member { get; set; }
        public string EncryptedData { get; set; }

        public override string GetReportHtml()
        {
            return "Emergency Contact information for <b>" + this.Member.FullName + "</b>";
        }
    }
}
