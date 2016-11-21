using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;

namespace Kcsara.Database.Web
{
  public class AuthorizeWithLogAttribute : AuthorizeAttribute
  {
    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
      var user = filterContext.HttpContext.User as System.Security.Claims.ClaimsPrincipal;
      var logger = LogManager.GetLogger(typeof(AuthorizeWithLogAttribute));
      if (user == null)
      {
        logger.DebugFormat(
          "Unathorized: {0} (no user)",
          filterContext.HttpContext.Request.RawUrl
          );
      }
      else
      {
        logger.DebugFormat(
          "Unauthorized: {0}\n[{1}]",
          filterContext.HttpContext.Request.RawUrl,
          string.Join("][", user.Claims.Select(f => f.Type + ": " + f.Value))
          );
      }
      base.HandleUnauthorizedRequest(filterContext);
    }

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
      return base.AuthorizeCore(httpContext);
    }

    public override void OnAuthorization(AuthorizationContext filterContext)
    {
      base.OnAuthorization(filterContext);
    }
  }
}