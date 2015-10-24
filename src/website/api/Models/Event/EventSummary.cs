using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.api.Models
{
  public class EventSummary
  {
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string StateNumber { get; set; }
    public string Location { get; set; }


    public DateTimeOffset Start { get; set; }
    public DateTimeOffset? Stop { get; set; }
  }
}