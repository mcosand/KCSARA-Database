using System;

namespace Sar.Database.Web.Auth
{
  public class LinkMemberRequest
  {
    public Guid MemberId { get; set; }
    public string Username { get; set; }
  }
}