using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models.Admin
{
  public class ExecuteDatabaseScript
  {
    public string UpdateKey { get; set; }
    public string Store { get; set; }
    public string Sql { get; set; }
  }
}