
namespace Kcsara.Database.Web.Model
{
    using System;
    using Kcsar.Database.Model;

    public class AnimalListRow
    {
        public Animal Animal { get; set; }
        public string PrimaryOwnerName { get; set; }
        public Member PrimaryOwner { get; set; }
        public DateTime? ActiveUntil { get; set; }
    }
}
