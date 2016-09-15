using System;
using System.Web.Optimization;
using System.Web.Routing;
using Kcsara.Database.Web;

namespace Sar.Auth
{
  public class AuthWebApplication : System.Web.HttpApplication
  {
    protected void Application_Start(object sender, EventArgs e)
    {
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);
    }
 }
}