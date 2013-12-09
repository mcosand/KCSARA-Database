
namespace Kcsara.Database.Web.Model
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace="")]
    public class AnimalView
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid? OwnerId { get; set; }
    }
}