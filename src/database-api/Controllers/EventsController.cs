using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [AnyHostCorsPolicy]
  public abstract class EventsController : ApiController
  {
    private readonly IEventsService _svc;
    private readonly IAuthorizationService _authz;

    protected EventsController(IEventsService svc, IAuthorizationService authz)
    {
      _svc = svc;
      _authz = authz;
    }

    [HttpGet]
    [Route("")]
    public Task<ListPermissionWrapper<GroupEventAttendance>> List()
    {
      return _svc.List(null);
    }
  }
}
