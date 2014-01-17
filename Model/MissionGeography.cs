/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.SqlServer.Types;
    using System.Data.SqlTypes;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MissionGeography : ModelObject
    {
        public Guid? InstanceId { get; set; }
        public string Kind { get; set; }
        public DateTime? Time { get; set; }
        public string Description { get; set; }
        public string LocationBinary { get; set; }
        public string LocationText { get; set; }
        public virtual Mission Mission { get; set; }

        private SqlGeography geog = null;
        [NotMapped]
        public SqlGeography Geography
        {
            get
            {
                if (geog == null)
                {
                    geog = this.LocationBinary == null ? null : SqlGeography.STGeomFromText(new SqlChars(this.LocationBinary.ToCharArray()), 4326);
                }
                return geog;
            }
            set
            {
                geog = value;
                this.LocationBinary = geog.ToString();
            }
        }

        #region IModelObject Members

        public override string GetReportHtml()
        {
            return "report";
        }

        #endregion

        #region IValidatedEntity Members

        public override bool Validate()
        {
            return true;
        }

        #endregion
    }
}
