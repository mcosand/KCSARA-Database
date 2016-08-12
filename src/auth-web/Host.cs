using System.Security.Claims;
using System.Web;
using Sar.Services;
using Sar.Services.Auth;

namespace Sar.Auth
{
  public class Host : CommonHost, IAuthenticatedHost
  {
    public string AccessToken
    {
      get
      {
        string header = HttpContext.Current.GetOwinContext().Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(header)) return null;
        string[] parts = header.Split(' ');
        if (parts.Length != 2 || parts[0] != "Bearer") return null;
        return parts[1];
      }
    }

    public ClaimsPrincipal User
    {
      get { return (ClaimsPrincipal)HttpContext.Current.GetOwinContext().Request.User; }
    }
  }
}