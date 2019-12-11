using System;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
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
    [AnyHostCorsPolicy]
    [Route("units")]
    public async Task<ListPermissionWrapper<Unit>> List()
    {
      await _authz.EnsureAsync(null, "Read:Unit");
      return await _units.List();
    }

    [HttpGet]
    [AnyHostCorsPolicy]
    [Route("units/{id}")]
    public async Task<ItemPermissionWrapper<Unit>> Get(Guid id)
    {
      await _authz.EnsureAsync(id, "Read:Unit");
      return await _units.Get(id);
    }

    [HttpPost]
    [AnyHostCorsPolicy]
    [ValidateModelState]
    [Route("units")]
    public async Task<Unit> CreateNew([FromBody]Unit unit)
    {
      await _authz.EnsureAsync(null, "Create:Unit");

      if (unit.Id != Guid.Empty)
      {
        throw new UserErrorException("New units shouldn't include an id");
      }

      unit = await _units.Save(unit);
      return unit;
    }

    [HttpPut]
    [AnyHostCorsPolicy]
    [ValidateModelState]
    [Route("units/{unitId}")]
    public async Task<Unit> Save(Guid unitId, [FromBody]Unit unit)
    {
      await _authz.EnsureAsync(unitId, "Update:Unit");

      if (unit.Id != unitId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      unit = await _units.Save(unit);
      return unit;
    }

    [HttpDelete]
    [AnyHostCorsPolicy]
    [Route("units/{unitId}")]
    public async Task Delete(Guid unitId)
    {
      await _authz.EnsureAsync(unitId, "Delete:Unit");

      await _units.Delete(unitId);
    }

    [HttpGet]
    [AnyHostCorsPolicy]
    [Route("units/{unitId}/reports")]
    public async Task<UnitReportInfo[]> ListReports(Guid unitId)
    {
      await _authz.EnsureAsync(unitId, "Read:Unit");
      return await _units.ListReports(unitId);
    }
  }
}
