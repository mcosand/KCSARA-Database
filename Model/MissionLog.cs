
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;

    public class MissionLog : ModelObject
    {
        public DateTime Time { get; set; }
        public string Data { get; set; }
        public virtual Mission Mission { get; set; }
        public virtual Member Person { get; set; }
        
        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }
            //    if (!this.MissionReference.IsLoaded)
            //    {
            //        this.MissionReference.Load();
            //    }
            //}
            return string.Format("<b>{0}</b> @{1}: {2} [{3}]", this.Mission.Title, this.Time, this.Data, (this.Person == null) ? "unknown" : this.Person.FullName);
        }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            //if (this.PersonReference.EntityKey == null && this.EntityState == System.Data.EntityState.Added)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Person", "", "Required"));
            //}

            //if (this.MissionReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Mission", "", "Required"));
            //}

            if (string.IsNullOrEmpty(this.Data))
            {
                errors.Add(new RuleViolation(this.Id, "Data", "", "Required"));
            }

            if (this.Time < KcsarContext.MinEntryDate)
            {
                errors.Add(new RuleViolation(this.Id, "Time", this.Time.ToString(), "Must be after " + KcsarContext.MinEntryDate.ToString()));
            }

            return (errors.Count == 0);
        }

        #endregion
    }
}
