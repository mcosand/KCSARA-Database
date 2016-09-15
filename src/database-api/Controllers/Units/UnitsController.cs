using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database;
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
    public async Task<IEnumerable<Unit>> List()
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, null, "Read:Unit")) throw new AuthorizationException();
      return await _units.List();
    }
  }
}
