// Based on https://gist.github.com/cherrydev/5c7168c8947eb1a1b5b8

namespace Kcsara.Database.Web
{
  using System.Security.Claims;
  using Microsoft.AspNet.Http;

  public interface ICurrentPrincipalProvider
  {
    ClaimsPrincipal CurrentPrincipal { get; }
  }

  public class CurrentPrincipalProvider : ICurrentPrincipalProvider
  {
    private IHttpContextAccessor _httpContextAccessor;

    public CurrentPrincipalProvider(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal CurrentPrincipal { get { return _httpContextAccessor.HttpContext.User; } }
  }
}
