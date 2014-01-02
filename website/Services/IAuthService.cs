namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using System.Web.Security;
  using System.Security.Principal;
  using System.Web.Profile;
  using Kcsar.Membership;
  using Kcsar.Database.Model;

  public interface IAuthService
  {
    Guid UserId { get; }
    bool IsSelf(Guid id);
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    bool IsUser { get; }
    bool IsInRole(params string[] group);
    bool IsMembershipForPerson(Guid id);
    bool IsMembershipForUnit(Guid id);
    bool IsUserOrLocal(HttpRequestBase request);
    bool IsRoleForPerson(string role, Guid personId);
    bool IsRoleForUnit(string role, Guid unitId);
  }
}