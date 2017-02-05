using System.Web.Http;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [Route("missions")]
  public class MissionsController : EventsController
  {
    public MissionsController(IMissionsService missions, IAuthorizationService authz) : base(missions, authz)
    {
    }
  }
}
