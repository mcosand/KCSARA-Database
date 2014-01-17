/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AnimalOwner : ModelObject
    {
        public bool IsPrimary { get; set; }
        public DateTime Starting { get; set; }
        public DateTime? Ending { get; set; }
        public virtual Animal Animal { get; set; }
        public virtual Member Owner { get; set; }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (! this.AnimalReference.IsLoaded)
            //    {
            //        this.AnimalReference.Load();
            //    }
            //    if (!this.OwnerReference.IsLoaded)
            //    {
            //        this.OwnerReference.Load();
            //    }
            //}
            return string.Format("<b>{0}</b> Suffix:{1} Type:{2} Comments:{3}", this.Animal.Name, this.Owner.FullName, this.IsPrimary, this.Starting, this.Ending);
        }

        #region IValidatedEntity Members
        public override bool Validate()
        {
            errors.Clear();

            //if (this.OwnerReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Owner", "", "Required"));
            //}

            //if (this.AnimalReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Animal", "", "Required"));
            //}

            if (this.Ending.HasValue && this.Ending <= this.Starting)
            {
                errors.Add(new RuleViolation(this.Id, "Ending", this.Ending.ToString(), "Must be after start date: " + this.Starting.ToString()));
            }

            return (errors.Count == 0);
        }

        #endregion
    }
}
