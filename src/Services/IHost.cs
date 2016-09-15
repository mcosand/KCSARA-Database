using System.Security.Claims;

namespace Kcsara.Database.Web.Services
{
  public interface IHost
  {
    ClaimsPrincipal User { get; }
    string AccessToken { get; }
    string GetConfig(string key);
  }
}
