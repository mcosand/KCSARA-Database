using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model.Units;
using Kcsara.Database.Services;
using Sar.Model;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class MembershipController : ApiController
  {
    private readonly IUnitsService _units;

    public MembershipController(IUnitsService units)
    {
      _units = units;
    }
    
    [HttpGet]
    [Route("members/{memberId}/memberships")]
    public Task<IEnumerable<UnitMembership>> ListForMember(Guid memberId, bool history = false)
    {
      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>> )(f => f.Member.Id == memberId)
        : (f => f.Member.Id == memberId && f.IsActive);

      return _units.ListMemberships(predicate);
    }

    [HttpGet]
    [Route("units/{unitId}/memberships")]
    public Task<IEnumerable<UnitMembership>> ListForUnit(Guid unitId, bool history = false)
    {
      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId)
        : (f => f.Unit.Id == unitId && f.IsActive);

      return _units.ListMemberships(predicate);
    }

    [HttpPost]
    [Route("members/{memberId}/memberships")]
    public async Task<UnitMembership> CreateForMember(Guid memberId, [FromBody] UnitMembership membership)
    {
      if (membership.Member == null) membership.Member = new NameIdPair();
      membership.Member.Id = memberId;

      return await _units.CreateMembership(membership);
    }
  }
}
