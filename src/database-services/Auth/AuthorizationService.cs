using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Sar.Database.Services
{
  public interface IAuthorizationService
  {
    Task<bool> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName);
    Task<bool> EnsureAsync(ClaimsPrincipal user, object resource, string policyName, bool throwIfDenied = true);
  }

  public class AuthorizationService : IAuthorizationService
  {
    private readonly IRolesService _rolesService;

    public AuthorizationService(IRolesService rolesService)
    {
      _rolesService = rolesService;
    }

    public Task<bool> EnsureAsync(ClaimsPrincipal user, object resource, string policyName, bool throwIfDenied = true)
    {
      var result = Authorize(user, resource, policyName);
      if (result == false && throwIfDenied) throw new AuthorizationException();
      return Task.FromResult(result);
    }

    public Task<bool> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
    {
      return Task.FromResult(Authorize(user, resource, policyName));
    }

    private bool Authorize(ClaimsPrincipal user, object resource, string policyName)
    {
      if (policyName == null) throw new ArgumentNullException(nameof(policyName));

      var memberIdString = user.FindFirst("memberId")?.Value;
      var memberId = string.IsNullOrWhiteSpace(memberIdString) ? (Guid?)null : new Guid(memberIdString);

      // Members can read their own records.
      if ((Guid?)resource == memberId && policyName.StartsWith("Read:") && (policyName.EndsWith("@Member") || policyName.EndsWith(":Member")))
      {
        return true;
      }

      Match m = Regex.Match(policyName, "^([a-zA-Z]+)(\\:([a-zA-Z]+)(@([a-zA-Z]+))?)?$");
      if (!m.Success) throw new InvalidOperationException("Unknown policy " + policyName);

      var roles = _rolesService.RolesForAccount(new Guid(user.FindFirst("sub").Value));
      var scopes = user.FindAll("scope").Select(f => f.Value).ToList();

      if (m.Groups[1].Value == "Read" && (
        roles.Any(f => f == "cdb.users")
        || scopes.Any(f => f.StartsWith("db-r-"))
        || scopes.Any(f => f.StartsWith("db-w-"))
        )) return true;

      if ((m.Groups[1].Value == "Create" || m.Groups[1].Value == "Update" || m.Groups[1].Value == "Delete")
        && (roles.Any(f => f == "cdb.admins") || scopes.Any(f => f.StartsWith("db-w-")))
        ) return true;

      return false;
    }
  }
}