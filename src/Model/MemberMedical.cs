﻿/*
 * Copyright 2013-2016 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
    public class MemberMedical : ModelObject
    {
        public virtual MemberRow Member { get; set; }
        public string EncryptedAllergies { get; set; }
        public string EncryptedMedications { get; set; }
        public string EncryptedDisclosures { get; set; }

        public override string GetReportHtml()
        {
            return "Medical information for <b>" + this.Member.FullName + "</b>";
        }

    }
}
