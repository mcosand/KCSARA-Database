using System.Configuration;
using System.Security.Claims;
using System.Web;
using Kcsara.Database.Services;

namespace Kcsara.Database.Web
{
  public class SystemWebHost : IHost
  {
    public ClaimsPrincipal User
    {
      get { return (ClaimsPrincipal)HttpContext.Current.User; }
    }

    public string AccessToken
    {
      get
      {
        string header = HttpContext.Current.Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(header)) return null;
        string[] parts = header.Split(' ');
        if (parts.Length != 2 || parts[0] != "Bearer") return null;
        return parts[1];
      }
    }

    public string GetConfig(string key)
    {
      return ConfigurationManager.AppSettings[key];
    }
  }
}