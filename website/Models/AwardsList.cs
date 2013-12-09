using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using Kcsar.Database.Model;

    public class AwardsList
    {
        public Member Member { get; set; }
        public IEnumerable<ITrainingAward> Awards { get; set; }
    }
}
