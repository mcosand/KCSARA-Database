using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model;
using Kcsara.Database.Services.Members;

namespace Kcsara.Database.Api.Controllers.Members
{
  public class MemberContactsController : ApiController
  {
    private readonly IMembersService _members;

    public MemberContactsController(IMembersService members)
    {
      _members = members;
    }

    [HttpGet]
    [Route("members/{memberId}/contacts")]
    [AnyHostCorsPolicy]
    public async Task<IEnumerable<PersonContact>> ListContacts(Guid memberId)
    {
      return await _members.ListMemberContactsAsync(memberId);
    }

    [HttpPost]
    [Route("members/{memberId}/contacts")]
    [AnyHostCorsPolicy]
    public async Task<PersonContact> CreateContact(Guid memberId, [FromBody] PersonContact contact)
    {
      return await _members.AddContact(memberId, contact);
    }
  }
}