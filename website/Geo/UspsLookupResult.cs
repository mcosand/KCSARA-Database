
namespace Kcsara.Database.Geo
{
    using Kcsar.Database.Model;

    public class UspsLookupResult : IAddress
    {
        public LookupResult Result { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
