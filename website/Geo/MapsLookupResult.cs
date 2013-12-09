
namespace Kcsara.Database.Geo
{
    using Kcsar.Database.Model;
    using Microsoft.SqlServer.Types;

    public class MapsLookupResult : IAddress
    {
        public LookupResult Result { get; set; }
        public int Quality { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public SqlGeography Geography { get; set; }
    }
}
