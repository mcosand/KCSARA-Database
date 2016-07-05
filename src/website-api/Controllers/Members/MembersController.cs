using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Services.Members;
using Kcsara.Database.Services.ObjectModel.Members;

namespace Kcsara.Database.Api.Controllers
{
  public class MembersController : ApiController
  {
    private readonly IMembersService _members;

    public MembersController(IMembersService members)
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
    [Authorize(Roles = "cdb.users")]
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