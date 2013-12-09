using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;
    using Kcsar.Database.Model;

    [DataContract]
    public class RecentDocumentsView
    {
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string DownloadUrl { get; set; }

    }
}
