/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;

    public class UnitMembership : ModelObject
    {
        public DateTime Activated { get; set; }
        public string Comments { get; set; }
        public virtual Member Person { get; set; }
        public virtual SarUnit Unit { get; set; }
        public virtual UnitStatus Status { get; set; }
        public DateTime? EndTime { get; set; }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }

            //    if (!this.UnitReference.IsLoaded)
            //    {
            //        this.UnitReference.Load();
            //    }

            //    if (!this.StatusReference.IsLoaded)
            //    {
            //        this.StatusReference.Load();
            //    }
            //}
            return string.Format("<b>{0}</b>, {1} member of <b>{2}</b> since {3:d}", this.Person.FullName, this.Status.StatusName, this.Unit, this.Activated);
        }

        public override string ToString()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }

            //    if (!this.UnitReference.IsLoaded)
            //    {
            //        this.UnitReference.Load();
            //    }

            //    if (!this.StatusReference.IsLoaded)
            //    {
            //        this.StatusReference.Load();
            //    }
            //}
            return string.Format("{0} [{1}][{2}", this.Person.FullName, this.Unit, this.Status.StatusName);
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
