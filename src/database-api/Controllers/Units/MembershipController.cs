using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Units;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class MembershipController : ApiController
  {
    private readonly IUnitsService _units;
    private readonly IAuthorizationService _authz;

    public MembershipController(IUnitsService units, IAuthorizationService authz)
    {
      _units = units;
      _authz = authz;
    }
    
    [HttpGet]
    [Route("members/{memberId}/memberships")]
    public async Task<IEnumerable<UnitMembership>> ListForMember(Guid memberId, bool history = false)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, memberId, "Read:UnitMembership@MemberId")) throw new AuthorizationException();

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>> )(f => f.Member.Id == memberId)
        : (f => f.Member.Id == memberId && f.IsActive);

      return await _units.ListMemberships(predicate);
    }

    [HttpGet]
    [Route("units/{unitId}/memberships")]
    public async Task<IEnumerable<UnitMembership>> ListForUnit(Guid unitId, bool history = false)
    {
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, unitId, "Read:UnitMembership@UnitId")) throw new AuthorizationException();

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId)
        : (f => f.Unit.Id == unitId && f.IsActive);

      return await _units.ListMemberships(predicate);
    }

    [HttpPost]
    [Route("members/{memberId}/memberships")]
    public async Task<UnitMembership> CreateForMember(Guid memberId, [FromBody] UnitMembership membership)
    {
      if (membership.Unit == null) throw new ArgumentException("unit is required");
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, membership.Unit.Id, "Create:UnitMembership@UnitId")) throw new AuthorizationException();
      if (!await _authz.AuthorizeAsync(User as ClaimsPrincipal, memberId, "Create:UnitMembership@MemberId")) throw new AuthorizationException();

      if (membership.Member == null) membership.Member = new MemberSummary();
      membership.Member.Id = memberId;

      return await _units.CreateMembership(membership);
    }
  }
}
