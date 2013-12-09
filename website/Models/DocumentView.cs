namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Runtime.Serialization;

    using Kcsar.Database.Model;

    [DataContract]
    public class DocumentView
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public Guid Reference { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Mime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? Changed { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string Thumbnail { get; set; }
    }
}
