/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class Document : ModelObject, IDocument
    {
        public static string StorageRoot { get; set; }
        public const int StorageTreeDepth = 2;
        public const int StorageTreeSpan = 100;

        public Guid ReferenceId { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int Size { get; set; }
        public string StorePath { get; set; }
        public string Description { get; set; }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //}
            return string.Format("<b>{0}</b>", this.FileName);
        }

        [NotMapped]
        public byte[] Contents
        {
            get
            {
                if (this._contents == null && !string.IsNullOrWhiteSpace(this.StorePath))
                {
                    this._contents = System.IO.File.ReadAllBytes(Document.StorageRoot + this.StorePath);
                }
                return this._contents;
            }
            set
            {
                this._contents = value;
                // If null, we'll set to empty string. KcsarContext takes care of setting this value when persisted to data store.
                this.StorePath = this.StorePath ?? string.Empty;
            }
        }
        private byte[] _contents = null;

        #region IValidatedEntity Members

        public override bool Validate()
        {
            errors.Clear();

            return (errors.Count == 0);
        }

        #endregion
    }
}
