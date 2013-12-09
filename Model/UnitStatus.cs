
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    public class UnitStatus : ModelObject
    {
        [Required]
        public string StatusName { get; set; }
        public int InternalWacLevel { get; set; }
        public bool IsActive { get; set; }
        public bool GetsAccount { get; set; }
        public virtual SarUnit Unit { get; set; }
        public virtual ICollection<UnitMembership> Memberships { get; set; }

        [DataMember]
        public WacLevel WacLevel
        {
            get
            {
                return (WacLevel)this.InternalWacLevel;
            }

            set
            {
                this.InternalWacLevel = (int)value;
            }
        }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.UnitReference.IsLoaded)
            //    {
            //        this.UnitReference.Load();
            //    }
            //}
            return string.Format("[<b>{0}</b>] [<b>{1}</b>] Active={2}, WAC={3}", this.Unit.DisplayName, this.StatusName, this.IsActive, this.WacLevel);
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
