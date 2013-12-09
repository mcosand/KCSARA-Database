namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TelemetryErrorView
    {
        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Location { get; set; }
    }
}
