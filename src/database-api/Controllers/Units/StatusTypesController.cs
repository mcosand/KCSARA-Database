using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database;
using Sar.Database.Model;
using Sar.Database.Model.Units;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class StatusTypesController : ApiController
  {
    private readonly IAuthorizationService _authz;
    private readonly IUnitsService _units;

    public StatusTypesController(IUnitsService units, IAuthorizationService authz)
    {
      _units = units;
      _authz = authz;
    }
    
    [HttpGet]
    [AnyHostCorsPolicy]
    [Route("units/statustypes")]
    public async Task<ListPermissionWrapper<UnitStatusType>> List()
    {
      await _authz.AuthorizeAsync(null, "Read:UnitStatusType");

      return await _units.ListStatusTypes();
    }

    [HttpGet]
    [AnyHostCorsPolicy]
    [Route("units/{unitId}/statustypes")]
    public async Task<ListPermissionWrapper<UnitStatusType>> ListForUnit(Guid unitId)
    {
      await _authz.EnsureAsync(unitId, "Read:UnitStatusType");

      return await _units.ListStatusTypes(unitId);
    }

    [HttpPost]
    [AnyHostCorsPolicy]
    [ValidateModelState]
    [Route("units/{unitId}/statusTypes")]
    public async Task<UnitStatusType> CreateNew(Guid unitId, [FromBody]UnitStatusType statusType)
    {
      await _authz.EnsureAsync(unitId, "Create:UnitStatusType@UnitId");

      if (statusType.Id != Guid.Empty)
      {
        throw new UserErrorException("New unit status shouldn't include an id");
      }

      if (statusType.Unit != null && statusType.Unit.Id != unitId)
      {
        throw new UserErrorException("Unit ids do not match", string.Format("Tried to save statusType with unit id {0} under unit {1}", statusType.Unit.Id, unitId));
      }

      statusType = await _units.SaveStatusType(statusType);
      return statusType;
    }

    [HttpPut]
    [AnyHostCorsPolicy]
    [ValidateModelState]
    [Route("units/{unitId}/statusTypes/{statusTypeId}")]
    public async Task<UnitStatusType> Save(Guid unitId, Guid statusTypeId, [FromBody]UnitStatusType statusType)
    {
      await _authz.EnsureAsync(statusTypeId, "Update:UnitStatusType");

      if (statusType.Unit.Id != unitId) ModelState.AddModelError("unit.id", "Can not be changed");
      if (statusType.Id != statusTypeId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      statusType = await _units.SaveStatusType(statusType);
      return statusType;
    }

    [HttpDelete]
    [AnyHostCorsPolicy]
    [Route("units/{unitId}/statusTypes/{statusTypeId}")]
    public async Task Delete(Guid unitId, Guid statusTypeId)
    {
      await _authz.EnsureAsync(statusTypeId, "Delete:UnitStatusType");

      await _units.DeleteStatusType(statusTypeId);
    }
  }
}
