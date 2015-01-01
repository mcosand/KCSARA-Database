/*
 * Copyright 2010-2014 Matthew Cosand
 */

//namespace Kcsar.Database.Model
//{
//    using System;
//    using System.Collections.Generic;

//    public partial class PersonSubscription : IModelObject
//    {
//        public PersonSubscription()
//            : base()
//        {
//            this.Id = Guid.NewGuid();          
//        }

//        public override string GetReportHtml()
//        {
//            if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
//            {
//                if (!this.PersonReference.IsLoaded)
//                {
//                    this.PersonReference.Load();
//                }
//                if (!this.ContactReference.IsLoaded)
//                {
//                    this.ContactReference.Load();
//                }
//            }
//            return string.Format("<b>{0}</b> @{1}: {2}",
//                (this.Person == null) ? "unknown" : this.Person.FullName,
//                this.ListName,
//                (this.Contact == null) ? "default" : this.Contact.Value);
//        }

//        #region IValidatedEntity Members

//        List<RuleViolation> errors = new List<RuleViolation>();

//        public override  IList<RuleViolation> Errors
//        {
//            get { return errors.AsReadOnly(); }
//        }

//        public override bool Validate()
//        {
//            errors.Clear();

//            if (this.PersonReference.EntityKey == null && this.EntityState == System.Data.EntityState.Added)
//            {
//                errors.Add(new RuleViolation(this.Id, "Person", "", "Required"));
//            }

//            if (this.ContactReference.EntityKey != null)
//            {
//                if (!this.ContactReference.IsLoaded)
//                {
//                    this.ContactReference.Load();
//                }
//                if (this.Contact.Type != "email")
//                {
//                    errors.Add(new RuleViolation(this.Id, "Contact", "", "Must use default or email contact"));
//                }
//            }

//            if (string.IsNullOrEmpty(this.ListName))
//            {
//                errors.Add(new RuleViolation(this.Id, "ListName", "", "Required"));
//            }

//            return (errors.Count == 0);
//        }

//        #endregion
//    }
//}
