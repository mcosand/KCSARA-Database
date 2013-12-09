using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsar.Database.Model
{
    public class SensitiveInfoAccess
    {
        public SensitiveInfoAccess()
        {
            this.Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        public string Actor { get; set; }
        public DateTime Timestamp { get; set; }
        public Member Owner { get; set; }
        public string Action { get; set; }
        public string Reason { get; set; }
    }
}
