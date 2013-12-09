
namespace Kcsar.Database.Model
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class UnitDocument : ModelObject
    {
        [Required]
        public string Url { get; set; }

        [Required]
        public string Title { get; set; }

        [ReportedReference]
        public SarUnit Unit { get; set; }

        [Required]
        public int Order { get; set; }

        public UnitDocumentType Type { get; set; }

        public string SubmitTo { get; set; }

        public bool Required { get; set; }

        public int? ForMembersOlder { get; set; }
        public int? ForMembersYounger { get; set; }

        public override string ToString()
        {
            return this.Unit.DisplayName + "'s " + this.Title;
        }

        public override string GetReportHtml()
        {
            return string.Format("{0} application document <b>{1}</b>", this.Unit.DisplayName, this.Title);
        }
    }
}