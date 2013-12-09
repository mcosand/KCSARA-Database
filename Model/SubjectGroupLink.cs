namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class SubjectGroupLink : ModelObject
    {
        public int Number { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual SubjectGroup Group { get; set; }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.SubjectReference.IsLoaded)
            //    {
            //        this.SubjectReference.Load();
            //    }

            //    if (!this.GroupReference.IsLoaded)
            //    {
            //        this.GroupReference.Load();
            //    }

            //    if (this.Group.Mission == null && !this.Group.MissionReference.IsLoaded)
            //    {
            //        this.Group.MissionReference.Load();
            //    }
           // }
            return string.Format("<b>{0}'s behavior on {1}</b> ", this.Subject.FirstName, this.Group.Mission.Title);
        }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            return (errors.Count == 0);
        }

        #endregion
    }
}
