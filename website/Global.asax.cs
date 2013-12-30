using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using Kcsar.Database.Model;
using Kcsara.Database.Web.Controllers;
using Ninject;
using Ninject.Web.Common;

namespace Kcsara.Database.Web
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : NinjectHttpApplication
  {
    protected void Session_Start(Object sender, EventArgs e)
    {
      decimal result;
      var browser = Request.Browser;
      if (browser.Browser == "IE" && decimal.TryParse(browser.Version, out result) && result < 8.0M)
      {
        Response.Write("This site requires Internet Explorer 8 or greater");
        Response.End();
      }
    }

    protected override void OnApplicationStarted()
    {
      base.OnApplicationStarted();

      AreaRegistration.RegisterAllAreas();

      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      Kcsar.Database.Model.Document.StorageRoot = Server.MapPath("~/content/auth/documents/");
    }

    protected override Ninject.IKernel CreateKernel()
    {
      IKernel kernel = new StandardKernel();
      kernel.Bind<IKcsarContext>().To<KcsarContext>();
      kernel.Bind<IFormsAuthentication>().To<FormsAuthenticationWrapper>();
      kernel.Bind<MembershipProvider>().ToMethod(context => System.Web.Security.Membership.Provider);
      return kernel;
    }
  }
}