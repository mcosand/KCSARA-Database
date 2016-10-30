using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using Kcsara.Database.Web.api;
using Ninject;

namespace Kcsara.Database.Web
{
  public class MvcApplication : HttpApplication
  {
    internal static IKernel kernel;

    protected void Application_Start()
    {
      SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));

      GlobalConfiguration.Configure(config => WebApiConfig.Register(config, kernel));
      BundleConfig.RegisterBundles(BundleTable.Bundles);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
    }

    protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
    {
      if (ClaimsPrincipal.Current is WindowsPrincipal)
      {
        Context.User = new ClaimsPrincipal(new ClaimsIdentity());
        Thread.CurrentPrincipal = Context.User;
      }
    }
  }
}
