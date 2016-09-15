using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kcsara.Database.Web.Services
{
  public interface IAuthorizationService
  {
    Task<bool> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName);
  }

  public class AuthorizationService : IAuthorizationService
  {
    private readonly IUserInfoService _rolesService;

    public AuthorizationService(IUserInfoService rolesService)
    {
      _rolesService = rolesService;
    }

    public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
    {
      var userInfo = await _rolesService.GetCurrentUserInfo();

      Match m = Regex.Match(policyName, "^([a-zA-Z]+)(\\:([a-zA-Z]+)(@([a-zA-Z]+))?)?$");
      if (!m.Success) throw new InvalidOperationException("Unknown policy " + policyName);

      
      if (m.Groups[1].Value == "Read" && m.Groups[3].Value == "TrainingRecord" && userInfo.Roles.Any(f => f == "cdb.users")) return true;

      return false;
    }
  }
}