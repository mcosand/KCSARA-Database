/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AnimalMission : ModelObject
    {
        public virtual Animal Animal { get; set; }
        public virtual MissionRoster MissionRoster { get; set; }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (! this.AnimalReference.IsLoaded)
            //    {
            //        this.AnimalReference.Load();
            //    }
            //    if (!this.MissionRosterReference.IsLoaded)
            //    {
            //        this.MissionRosterReference.Load();
            //    }

            //    if (this.MissionRoster != null && !this.MissionRoster.MissionReference.IsLoaded)
            //    {
            //        this.MissionRoster.MissionReference.Load();
            //    }
            //}
            return string.Format("<b>[{0}] [{1}]</b> In:{2} Out:{3}", this.MissionRoster.Mission.Title, this.Animal.Name, this.MissionRoster.TimeIn, this.MissionRoster.TimeOut);
        }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            //if (this.MissionRoster == null && this.MissionRosterReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "MissionRoster", "", "Required"));
            //}

            //if (this.Animal == null && this.AnimalReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Animal", "", "Required"));
            //}

            return (errors.Count == 0);
        }

        #endregion
    }
}
