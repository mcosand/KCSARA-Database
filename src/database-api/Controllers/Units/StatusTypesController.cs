using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model.Units;
using Kcsara.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class StatusTypesController : ApiController
  {
    private readonly IUnitsService _units;

    public StatusTypesController(IUnitsService units)
    {
      _units = units;
    }
    
    [HttpGet]
    [Route("units/statustypes")]
    public Task<IEnumerable<UnitStatusType>> List()
    {
      return _units.ListStatusTypes();
    }
  }
}
