/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class PersonContact : ModelObject
    {
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string Value { get; set; }
        public int Priority { get; set; }
        public virtual Member Person { get; set; }

        public static readonly string[] AllowedTypes = new string[] { "email", "phone", "hamcall", "im" };

        public static bool TryParse(string type, string subtype, string input, bool strict, out string result)
        {
            result = input;

            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (type.ToLower() == "phone")
            {
                // Allow custom (international) numbers if they start with '+'
                if (input[0] == '+')
                {
                    return true;
                }

                string pattern = strict ? @"^\d{3}\-\d{3}-\d{4}( x\d+)?$" : @"^\(?(\d{3})\)?[ \.\-]*(\d{3})[ \.\-]*(\d{4})(\s*e?xt?\s*(\d+))?$";
                Match m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (!m.Success)
                {
                    return false;
                }
               
                if (!strict)
                {
                    result = string.Format("{0}-{1}-{2}{3}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, "");
                }

                return true;
            }
            else if (type.ToLower() == "hamcall")
            {
                if (!strict)
                {
                    result = input.ToUpper();
                }
                return true;
            }
            else if (type.ToLower() == "email")
            {
                // Pattern from http://www.regular-expressions.info/email.html
                string pattern = @"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";

                return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
            }

            return true;
        }

        public static ContactInfoSubType GetSubTypes(string type)
        {
            ContactInfoSubType subType = new ContactInfoSubType() { ValueLabel = "Value", SubTypes = new string[0], ValidationString = "" };

            switch (type.ToLower())
            {
                case "phone":
                    subType.SubTypes = new string[] { "cell", "home", "work", "pager" };
                    subType.ValueLabel = "Number";
                    break;
                case "im":
                    subType.SubTypes = new string[] { "messenger", "aim", "yahoo", "google", "twitter" };
                    subType.ValueLabel = "Address";
                    break;
                case "email":
                    subType.ValueLabel = "Address";
                    break;
                case "hamcall":
                    subType.ValueLabel = "Call Sign";
                    break;
            }

            return subType;
        }

        public PersonContact()
            : base()
        {
            this.Type = PersonContact.AllowedTypes[0];
            this.Subtype = PersonContact.GetSubTypes(this.Type).SubTypes.FirstOrDefault() ?? "";
        }

        public override string ToString()
        {
            return this.GetReportHtml();
        }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }
            //}
            return string.Format("<b>{0}</b> {1} contact: {2} {3}", this.Person.FullName, this.Type, this.Subtype, this.Value);
        }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            //if (this.Person == null && this.PersonReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Person", "", "Required"));
            //}

            if (string.IsNullOrEmpty(this.Type))
            {
                errors.Add(new RuleViolation(this.Id, "Type", "", "Required"));
            }
            else if (! PersonContact.AllowedTypes.Contains(this.Type.ToLower()))
            {
                errors.Add(new RuleViolation(this.Id, "Type", this.Type, "Must be one of: " + string.Join(", ", PersonContact.AllowedTypes)));
            }

            ContactInfoSubType subtypes = PersonContact.GetSubTypes(this.Type);
            if (subtypes.SubTypes.Length == 0 && !string.IsNullOrEmpty(this.Subtype))
            {
                errors.Add(new RuleViolation(this.Id, "SubType", this.Subtype, "SubType can't be specified for type '" + this.Type + "'"));
            }
            else if (subtypes.SubTypes.Length > 0 && !subtypes.SubTypes.Contains(this.Subtype))
            {
                errors.Add(new RuleViolation(this.Id, "SubType", this.Subtype, "SubType must be one of: " + string.Join(", ", subtypes.SubTypes)));
            }

            if (string.IsNullOrEmpty(this.Value))
            {
                errors.Add(new RuleViolation(this.Id, "Value", "", "Required"));
            }
            
            string dummy;
            if (!PersonContact.TryParse(this.Type, this.Subtype, this.Value, true, out dummy))
            {
                errors.Add(new RuleViolation(this.Id, "Value", this.Value, string.Format("'{0}' is not in valid form", this.Value)));
            }

            return (errors.Count == 0);
        }

        #endregion
    }
}
