namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.SqlServer.Types;
    using System.Data.SqlTypes;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PersonAddress : ModelObject, IAddressGeography
    {
        public string InternalType { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Geo { get; set; }
        public int Quality { get; set; }
        public int RosterVisibility { get; set; }
        public virtual Member Person { get; set; }

        public string SimpleText { get { return string.Format("{0}, {1}, {2} {3}", this.Street, this.City, this.State, this.Zip); } }

        [DataMember]
        [NotMapped]
        public PersonAddressType Type
        {
            get
            {
                PersonAddressType type = PersonAddressType.Other;

                if (string.IsNullOrEmpty(this.InternalType))
                {
                    return type;
                }
                try
                {
                    type = (PersonAddressType)Enum.Parse(typeof(PersonAddressType), this.InternalType, true);
                }
                catch (ArgumentException)
                {
                    type = PersonAddressType.Other;
                }

                return type;
            }

            set
            {
                this.InternalType = value.ToString().ToLower();
            }
        }

        private SqlGeography geog = null;
        [NotMapped]
        public SqlGeography Location
        {
            get
            {
                if (geog == null)
                {
                    geog = this.Geo == null ? null : SqlGeography.STGeomFromText(new SqlChars(this.Geo.ToCharArray()), 4326);
                }
                return geog;
            }
            set
            {
                geog = value;
                this.Geo = (geog == null) ? null : geog.ToString();
            }
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
            return string.Format("<b>{0}</b> {1} address: [{2}] [{3}], [{4}] [{5}]", this.Person.FullName, this.Type, this.Street, this.City, this.State, this.Zip);
        }

        public override string ToString()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }
            //}
            return string.Format("{0} {1}: {2}, {3}, {4}", this.Person.FullName, this.Type, this.Street, this.City, this.Zip);
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
