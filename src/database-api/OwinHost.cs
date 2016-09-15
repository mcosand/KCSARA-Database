using System.Security.Claims;
using System.Web;
using Sar.Services;
using Sar.Services.Auth;

namespace Kcsara.Database.Api
{
  public class OwinHost : CommonHost, IAuthenticatedHost
  {
    public OwinHost()
    {
    }

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