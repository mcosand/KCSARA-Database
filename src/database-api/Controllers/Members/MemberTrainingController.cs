using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Training;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Members
{
  public class MembersTrainingController : ApiController
  {
    private readonly IMembersService _members;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    public MembersTrainingController(IMembersService members, IAuthorizationService authz, IHost host)
    {
      _members = members;
      _authz = authz;
      _host = host;
    }

    [HttpGet]
    [Route("members/{memberId}/trainings/stats")]
    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");

      return await _members.GetTrainingStatistics(memberId);
    }

    [HttpGet]
    [Route("members/{memberId}/trainings")]
    public async Task<List<EventAttendance>> ListMissions(Guid memberId)
    {
      await _authz.EnsureAsync(memberId, "Read:Member");
      await _authz.EnsureAsync(null, "Read:Training");

      return await _members.GetTrainingList(memberId);
    }
  }
}
