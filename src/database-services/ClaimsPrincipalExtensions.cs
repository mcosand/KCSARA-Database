using System.Security.Claims;
using System.Security.Principal;

namespace Sar.Database.Services
{
  public static class ClaimsPrincipalExtensions
  {
    public static string GetAccessToken(this IPrincipal principal)
    {
      var user = principal as ClaimsPrincipal;
      if (user == null) return null;

      return user.FindFirst("access_token")?.Value;
    }
  }
}
