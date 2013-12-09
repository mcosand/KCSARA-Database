using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Kcsara.Database.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Kcsar.Database.Model.Document.StorageRoot = Server.MapPath("~/content/auth/documents/");
        }

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
    }
}