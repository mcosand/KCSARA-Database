using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model;
using Kcsara.Database.Services.Members;
using Sar.Services.Auth;

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
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, memberId, "Read:Member")) throw new AuthorizationException();

      return await _members.ListMemberContactsAsync(memberId);
    }

    [HttpPost]
    [Route("members/{memberId}/contacts")]
    [AnyHostCorsPolicy]
    public async Task<PersonContact> CreateContact(Guid memberId, [FromBody] PersonContact contact)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, memberId, "Create:MemberContact@Member")) throw new AuthorizationException();
      return await _members.AddContact(memberId, contact);
    }
  }
}