
namespace Kcsar.Database.Model
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class MemberUnitDocument : ModelObject
    {
        [Required]
        [ReportedReference]
        public Member Member { get; set; }

        [Required]
        [ReportedReference]
        public UnitDocument Document { get; set; }

        public DateTime? MemberAction { get; set; }
        
        public DateTime? UnitAction { get; set; }

        public DocumentStatus Status { get; set; }

        public override string ToString()
        {
            return string.Format("{0}'s unit document: {1}/{2}", this.Member.FullName, this.Document.Unit.DisplayName, this.Document.Title);
        }

        public override string GetReportHtml()
        {
            return string.Format("{0} unit document {1}/<b>{2}</b> {3}", this.Member.FullName, this.Document.Unit.DisplayName, this.Document.Title, this.Status);
        }
    }
}