using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database;
using Sar.Database.Model;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Members
{
  public class MemberContactsController : ApiController
  {
    private readonly IMembersService _members;
    private readonly IAuthorizationService _authz;

    public MemberContactsController(IMembersService members, IAuthorizationService authz)
    {
      _members = members;
      _authz = authz;
    }

    [HttpGet]
    [Route("members/{memberId}/contacts")]
    [AnyHostCorsPolicy]
    public async Task<IEnumerable<PersonContact>> ListContacts(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.ListMemberContactsAsync(memberId);
    }

    [HttpPost]
    [Route("members/{memberId}/contacts")]
    [AnyHostCorsPolicy]
    public async Task<PersonContact> CreateContact(Guid memberId, [FromBody] PersonContact contact)
    {
      await _authz.EnsureAsync(memberId, "Create:MemberContact@MemberId");
      return await _members.AddContact(memberId, contact);
    }
  }
}