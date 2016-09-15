/*
 * Copyright Matthew Cosand
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model.Members;
using Sar.Database.Services;

namespace Sar.Database.Api.Controllers
{
  public class MembersApiController : ApiController
  {
    private readonly IMembersService _members;

    public MembersApiController(IMembersService members)
    {
      _members = members;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Members/ByWorkerNumber/{id}")]
  //  [Authorize(Roles = "cdb.users")]
  [AllowAnonymous]
    public async Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id)
    {
      return await _members.ByWorkerNumber(id);
    }

    [HttpGet]
    [Route("Members/ByPhoneNumber/{id}")]
    [Authorize(Roles = "cdb.users")]
    public async Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id)
    {
      return await _members.ByPhoneNumber(id);
    }
  }
}