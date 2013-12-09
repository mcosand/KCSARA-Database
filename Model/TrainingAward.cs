
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading;

    public class TrainingAward : ModelObject, ITrainingAward
    {
        [Required]
        public DateTime Completed { get; set; }
        public DateTime? Expiry { get; set; }
        public string DocPath { get; set; }
        public string metadata { get; set; }
        [Required]
        public virtual Member Member { get; set; }
        [Required]
        public virtual TrainingCourse Course { get; set; }
        public virtual TrainingRoster Roster { get; set; }

        
        [NotMapped]
        public bool UploadsPending { get; set; }

        public TrainingAward()
            : base()
        {
            this.LastChanged = DateTime.Now;
            this.ChangedBy = Thread.CurrentPrincipal.Identity.Name;
            this.UploadsPending = false;
            this.Completed = DateTime.Today;
        }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.MemberReference.IsLoaded)
            //    {
            //        this.MemberReference.Load();
            //    }
            //    if (!this.CourseReference.IsLoaded)
            //    {
            //        this.CourseReference.Load();
            //    }
            //}

            return string.Format("<b>{0}</b> awarded <b>{1}</b>, {2:d}. Expires:{3:d}, Have Roster={4}", this.Member.FullName, this.Course.DisplayName, this.Completed, this.Expiry, this.Roster != null);
        }

        public override string ToString()
        {
            return this.GetReportHtml();
        }


        public DateTime? NullableCompleted { get { return new DateTime?(this.Completed); } }

        public TrainingRule Rule { get { return null; } }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            //if (this.Member == null && this.MemberReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Member", "", "Required"));
            //}

            //if (this.Course == null && this.CourseReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Course", "", "Required"));
            //}

            if (this.Completed > DateTime.Today.AddMonths(3))
            {
                errors.Add(new RuleViolation(this.Id, "Completed", this.Completed.ToString(), "Too far in the future. Must be less than 3 months from today."));
            }

            if (this.Expiry.HasValue && this.Completed > this.Expiry)
            {
                errors.Add(new RuleViolation(this.Id, "Expiry", this.Expiry.ToString(), "Expiration must be after time completed."));
            }

            //if (this.RosterReference.EntityKey != null)
            //{
            //    if ((this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged) && !this.RosterReference.IsLoaded)
            //    {
            //        this.RosterReference.Load();
            //    }
            //    if (this.Completed != this.Roster.TimeOut)
            //    {
            //        errors.Add(new RuleViolation(this.Id, "Completed", this.Completed.ToString(), "When an award is via a roster, Completed must be the roster time out."));
            //    }
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(this.metadata) && this.Roster == null && !this.UploadsPending)
            //    {
            //        errors.Add(new RuleViolation(this.Id, "metadata", "", "When an award is given without a roster, documentation is required."));
            //    }
            //}
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{

            //    if (this.Member == null && !this.MemberReference.IsLoaded)
            //    {
            //        this.MemberReference.Load();
            //    }

            //    if (this.Course == null && !this.CourseReference.IsLoaded)
            //    {
            //        this.CourseReference.Load();
            //    }

            //    if (this.Member != null && !this.Member.TrainingAwards.IsLoaded)
            //    {
            //        this.Member.TrainingAwards.Load();
            //    }
            //}

            //foreach (TrainingAward row in this.Member.TrainingAwards)
            //{
            //    if (row.Id != this.Id && this.MemberReference.EntityKey == row.MemberReference.EntityKey && this.CourseReference.EntityKey == row.CourseReference.EntityKey && this.Completed == row.Completed)
            //    {
            //        errors.Add(new RuleViolation(this.Id, "Completed", this.Completed.ToString(),
            //            string.Format("Conflicts with another entry, where Course \"{0}\" was completed {1}", this.Course.DisplayName, this.Completed)));
            //    }
            //}


            return (errors.Count == 0);
        }

        #endregion
    }
}
