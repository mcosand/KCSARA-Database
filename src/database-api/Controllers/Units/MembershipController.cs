using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
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
    public async Task<ListPermissionWrapper<UnitMembership>> ListForMember(Guid memberId, bool history = false)
    {
      await _authz.EnsureAsync(memberId, "Read:UnitMembership@MemberId");

      DateTimeOffset now = DateTimeOffset.UtcNow;

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>> )(f => f.Member.Id == memberId)
        : (f => f.Member.Id == memberId && f.IsActive && (f.End == null || f.End > now));

      return await _units.ListMemberships(predicate, _authz.CanCreateMembershipForMember(memberId));
    }

    [HttpGet]
    [Route("units/{unitId}/memberships")]
    public async Task<ListPermissionWrapper<UnitMembership>> ListForUnit(Guid unitId, bool history = false)
    {
      DateTimeOffset now = DateTimeOffset.UtcNow;

      await _authz.EnsureAsync(unitId, "Read:UnitMembership@UnitId");

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId)
        : (f => f.Unit.Id == unitId && f.IsActive && (f.End == null || f.End > now));

      return await _units.ListMemberships(predicate, true);
    }

    [HttpGet]
    [AnyHostCorsPolicy]
    [Route("units/{unitId}/memberships/byStatus/{statusName}")]
    public async Task<ListPermissionWrapper<UnitMembership>> ListForUnit(Guid unitId, string statusName, bool history = false)
    {
      DateTimeOffset now = DateTimeOffset.UtcNow;

      await _authz.EnsureAsync(unitId, "Read:UnitMembership@UnitId");

      Expression<Func<UnitMembership, bool>> predicate = history
        ? (Expression<Func<UnitMembership, bool>>)(f => f.Unit.Id == unitId && f.Status == statusName)
        : (f => f.Unit.Id == unitId && f.IsActive && (f.End == null || f.End > now) && f.Status == statusName);

      return await _units.ListMemberships(predicate, true);
    }

    [HttpPost]
    [Route("members/{memberId}/memberships")]
    public async Task<UnitMembership> CreateForMember(Guid memberId, [FromBody] UnitMembership membership)
    {
      if (membership.Unit == null) throw new ArgumentException("unit is required");
      await _authz.AuthorizeAsync(membership.Unit.Id, "Create:UnitMembership@UnitId");
      await _authz.AuthorizeAsync(memberId, "Create:UnitMembership@MemberId");

      if (membership.Member == null) membership.Member = new MemberSummary();
      membership.Member.Id = memberId;

      return await _units.CreateMembership(membership);
    }
  }
}
