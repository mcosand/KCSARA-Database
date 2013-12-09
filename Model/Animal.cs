namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Animal : ModelObject
    {
        public static readonly string[] AllowedTypes = new string[] { "horse", "dog" };

        public string DemSuffix { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Comments { get; set; }
        public virtual ICollection<AnimalOwner> Owners { get; set; }
        public virtual ICollection<AnimalMission> MissionRosters { get; set; }
        public string PhotoFile { get; set; }

        public Animal()
            : base()
        {
            this.Type = Animal.AllowedTypes[0];
            this.Owners = new List<AnimalOwner>();
            this.MissionRosters = new List<AnimalMission>();
        }

        public Member GetPrimaryOwner()
        {
            //if (!this.Owners.IsLoaded)
            //{
            //    this.Owners.Load();
            //}

            foreach (AnimalOwner link in this.Owners)
            {
                if (link.IsPrimary)
                {
                    //if (!link.OwnerReference.IsLoaded)
                    //{
                    //    link.OwnerReference.Load();
                    //}
                    return link.Owner;
                }
            }
            return null;
        }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
                
            //}
            return string.Format("<b>{0}</b> Suffix:{1} Type:{2} Comments:{3}", this.Name, this.DemSuffix, this.Type, this.Comments);
        }

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            if (string.IsNullOrEmpty(this.Name))
            {
                errors.Add(new RuleViolation(this.Id, "Name", "", "Required"));
            }

            if (string.IsNullOrEmpty(this.DemSuffix))
            {
                errors.Add(new RuleViolation(this.Id, "DemSuffix", "", "Required"));
            }

            if (string.IsNullOrEmpty(this.Type))
            {
                errors.Add(new RuleViolation(this.Id, "Type", "", "Required"));
            }
            else if (!Animal.AllowedTypes.Contains(this.Type.ToLower()))
            {
                errors.Add(new RuleViolation(this.Id, "Type", this.Type, "Must be one of: " + string.Join(", ", Animal.AllowedTypes)));
            }

            return (errors.Count == 0);
        }

        #endregion
    }
}
