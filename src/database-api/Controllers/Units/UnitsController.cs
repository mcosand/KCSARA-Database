using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model.Units;
using Kcsara.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class UnitsController : ApiController
  {
    private readonly IUnitsService _units;

    public UnitsController(IUnitsService units)
    {
      _units = units;
    }

    [HttpGet]
    [Route("units")]
    public async Task<IEnumerable<Unit>> List()
    {
      return await _units.List();
    }
  }
}
