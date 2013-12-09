
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class TrainingCourse : ModelObject
    {
        [Required]
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Categories { get; set; }
        public int WacRequired { get; set; }
        public bool ShowOnCard { get; set; }
        public int? ValidMonths { get; set; }
        public DateTime? OfferedFrom { get; set; }
        public DateTime? OfferedUntil { get; set; }
        public string Metadata { get; set; }
        public virtual ICollection<TrainingAward> TrainingAward { get; set; }
        public virtual SarUnit Unit { get; set; }
        public virtual ICollection<ComputedTrainingAward> ComputedAwards { get; set; }
        public virtual ICollection<Training> Trainings { get; set; }
        public string PrerequisiteText { get; set; }
        
        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //}
            return string.Format("<b>{0}</b> [Card={1}] [WAC={2}] [Months={3}] From {4} to {5}", this.DisplayName, this.ShowOnCard, this.WacRequired, this.ValidMonths, this.OfferedFrom, this.OfferedUntil);
        }

        public IEnumerable<Member> GetEligibleMembers(IEnumerable<Member> availableMembers, bool asRefresher)
        {
            if (string.IsNullOrEmpty(this.PrerequisiteText))
            {
                return availableMembers;
            }

            List<Member> members = new List<Member>();

            foreach (Member m in availableMembers)
            {
                //if (!m.ComputedAwards.IsLoaded)
                //{
                //    m.ComputedAwards.Load();
                //}

                if (asRefresher == false &&
                    (from c in m.ComputedAwards where c.Course.Id == this.Id && (c.Expiry == null || c.Expiry > DateTime.Today) select c).FirstOrDefault() != null)
                {
                    continue;
                }

                bool add = true;

                string[] fields = this.PrerequisiteText.Split('+');

                foreach (string field in fields)
                {
                    if (field.StartsWith("Mission"))
                    {
                        throw new NotImplementedException("Don't know how to process mission hours as course requisite");
                    }
                    else if (field.StartsWith("Background"))
                    {
                        if (!m.BackgroundDate.HasValue || m.BackgroundDate < DateTime.Today.AddYears(-2))
                        {
                            add = false;
                            break;
                        }
                    }
                    else if (field.StartsWith("SheriffApp"))
                    {
                        if (!m.SheriffApp.HasValue || m.SheriffApp < DateTime.Today.AddMonths(-18))
                        {
                            add = false;
                            break;
                        }
                    }
                    else
                    {
                        Guid? sourceCourse = fields[0].ToGuid();

                        if (sourceCourse == null)
                        {
                            throw new InvalidOperationException("Unknown prequisite for course: " + this.DisplayName);
                        }

                        if ((from f in m.ComputedAwards where (f.Expiry == null || f.Expiry >= DateTime.Today) && f.Course.Id == sourceCourse.Value select f).FirstOrDefault() == null)
                        {
                            add = false;
                            break;
                        }
                    }
                }
                if (add)
                {
                    members.Add(m);
                }
            }

            return members;
        }


        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            if (string.IsNullOrEmpty(this.DisplayName))
            {
                errors.Add(new RuleViolation(this.Id, "DisplayName", "", "Required"));
            }

            if (string.IsNullOrEmpty(this.FullName))
            {
                errors.Add(new RuleViolation(this.Id, "FullName", "", "Required"));
            }

            if (this.OfferedFrom != null && this.OfferedUntil != null)
            {
                if (this.OfferedFrom >= this.OfferedUntil)
                {
                    errors.Add(new RuleViolation(this.Id, "OfferedUntil", this.OfferedUntil.ToString(), "Must be after Offered From: " + this.OfferedFrom.ToString()));
                }
            }

            return (errors.Count == 0);
        }

        #endregion
    }
}
