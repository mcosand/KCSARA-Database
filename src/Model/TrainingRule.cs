/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;

namespace Kcsar.Database.Model
{
    public class TrainingRule : ModelObject
    {
        public string RuleText { get; set; }
        public DateTime? OfferedFrom { get; set; }
        public DateTime? OfferedUntil { get; set; }
        public virtual ICollection<TrainingAwardRow> Results { get; set; }

        public override string GetReportHtml()
        {
            return RuleText;
        }
    }
}
