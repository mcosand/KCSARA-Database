using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    public class UnitApplicationInfoViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NoAppReason { get; set; }
        public bool IsAcceptingApps
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.NoAppReason);
            }
        }
        public string Contact { get; set; }
    }
}