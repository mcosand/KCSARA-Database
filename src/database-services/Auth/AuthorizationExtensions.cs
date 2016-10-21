using System;
using Sar.Database.Model.Units;

namespace Sar.Database.Services
{
  public static class AuthorizationExtensions
  {
    public static bool CanCreateStatusForUnit(this IAuthorizationService authz, Guid? unitId)
    {
      return unitId.HasValue ? authz.Authorize(unitId.Value, "Create:UnitStatusType@UnitId") : authz.CanCreate<UnitStatusType>();
    }

    public static bool CanCreateMembershipForUnit(this IAuthorizationService authz, Guid? unitId)
    {
      return unitId.HasValue ? authz.Authorize(unitId.Value, "Create:UnitMembership@UnitId") : authz.CanCreate<UnitMembership>();
    }

    public static bool CanCreateMembershipForMember(this IAuthorizationService authz, Guid? memberId)
    {
      return memberId.HasValue ? authz.Authorize(memberId.Value, "Create:UnitMembership@MemberId") : authz.CanCreate<UnitMembership>();
    }
  }
}
