using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model;
using Sar.Database.Model.Events;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [AnyHostCorsPolicy]
  public abstract class EventsController : ApiController
  {
    private readonly IEventsService _svc;
    private readonly IAuthorizationService _authz;
    protected readonly string _eventType;

    protected EventsController(string eventType, IEventsService svc, IAuthorizationService authz)
    {
      _eventType = eventType;
      _svc = svc;
      _authz = authz;
    }

    [HttpGet]
    [Route("years")]
    public async Task<List<EventYearSummary>> Years()
    {
      await _authz.EnsureAsync(null, "Read:" + _eventType);
      return await _svc.ListYears();
    }

    [HttpGet]
    [Route("")]
    public async Task<ListPermissionWrapper<GroupEventAttendance>> List(int? year = null)
    {
      await _authz.EnsureAsync(null, "Read:" + _eventType);
      return await (year.HasValue ? _svc.ListForYear(year.Value) : _svc.List(f => true));
    }

    [HttpDelete]
    [Route("{eventid}")]
    public async Task Delete(Guid eventId)
    {
      await _authz.EnsureAsync(eventId, "Delete:" + _eventType);
      await _svc.Delete(eventId);
    }
  }
}
