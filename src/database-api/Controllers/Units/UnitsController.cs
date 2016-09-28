using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database;
using Sar.Database.Api.Extensions;
using Sar.Database.Model;
using Sar.Database.Model.Units;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class UnitsController : ApiController
  {
    private readonly IAuthorizationService _authz;
    private readonly IUnitsService _units;

    public UnitsController(IUnitsService units, IAuthorizationService authz)
    {
      _units = units;
      _authz = authz;
    }

    [HttpGet]
    [Route("units")]
    public async Task<ListPermissionWrapper<Unit>> List()
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, null, "Read:Unit");
      return await _units.List();
    }

    [HttpGet]
    [Route("units/{id}")]
    public async Task<ItemPermissionWrapper<Unit>> Get(Guid id)
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, id, "Read:Unit");
      return await _units.Get(id);
    }

    [HttpGet]
    [Route("units/{unitId}/reports")]
    public async Task<UnitReportInfo[]> ListReports(Guid unitId)
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, unitId, "Read:Unit");
      return await _units.ListReports(unitId);
    }
  }
}
