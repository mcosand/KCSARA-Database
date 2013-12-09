namespace Kcsar.Database.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class xref_county_id
    {
        [Key, Column(Order=0)]
        public int accessMemberID { get; set; }
        [Key, Column(Order=1)]
        public Guid personId { get; set; }
        [Key, Column(Order=2)]
        public string ExternalSource { get; set; }
    }
}
