using System.Web.Http;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [RoutePrefix("missions")]
  public class MissionsController : EventsController
  {
    public MissionsController(IMissionsService missions, IAuthorizationService authz) : base("Mission", missions, authz)
    {
    }
  }
}
