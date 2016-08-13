using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sar.Auth
{
  public class LinkMemberRequest
  {
    public Guid MemberId { get; set; }
    public string Username { get; set; }
  }
}