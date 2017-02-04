using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [AnyHostCorsPolicy]
  public class TrainingsController : ApiController
  {
    private readonly ITrainingsService _trainings;
    private readonly IAuthorizationService _authz;

    public TrainingsController(ITrainingsService trainings, IAuthorizationService authz)
    {
      _trainings = trainings;
      _authz = authz;
    }

    [HttpGet]
    [Route("trainings")]
    public Task<ListPermissionWrapper<GroupEventAttendance>> List()
    {
      return _trainings.List(null);
    }
  }
}
