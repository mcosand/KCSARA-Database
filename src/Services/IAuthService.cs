namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Web;

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
