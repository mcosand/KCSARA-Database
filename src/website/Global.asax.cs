/*
 * Copyright 2008-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System;
  using System.Security.Principal;
  using System.Threading;
  using System.Web;
  using System.Web.Http;
  using System.Web.Mvc;
  using System.Web.Optimization;
  using System.Web.Routing;
  using Kcsar.Database.Model;
  using Kcsara.Database.Extensions;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.api;
  using log4net;
  using Ninject;
  using Ninject.Web.Common;

  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : NinjectHttpApplication
  {
    // This is static until legacy code learns to ask for dependencies in constructors
    public static IKernel myKernel;
    static MvcApplication()
    {
      log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));

      myKernel = new StandardKernel();
      myKernel.Bind<IKcsarContext>().To<KcsarContext>();
      myKernel.Bind<ILog>().ToMethod(context => LogManager.GetLogger("Default"));
      myKernel.Bind<IFormsAuthentication>().To<FormsAuthenticationWrapper>();
      myKernel.Bind<IAuthService>().To<AuthService>();
      myKernel.Bind<IPrincipal>().ToMethod(f => Thread.CurrentPrincipal);
      myKernel.Bind<Func<IPrincipal>>().ToConstant((Func<IPrincipal>)(() => Thread.CurrentPrincipal));
      myKernel.Bind<IAppSettings>().To<AppSettings>();
      myKernel.Bind<IReportsService>().To<ReportsService>();
      myKernel.Bind<IExtensionProvider>().To<ExtensionProvider>().InSingletonScope();
      myKernel.Get<IExtensionProvider>().Initialize();

      myKernel.Bind<Func<IKcsarContext>>().ToConstant((Func<IKcsarContext>)(() => new KcsarContext()));

      myKernel.Bind<AccountsService>().ToSelf().InSingletonScope();
    }

    protected void Session_Start(object sender, EventArgs e)
    {
      decimal result;
      var browser = Request.Browser;
      if (browser.Browser == "IE" && decimal.TryParse(browser.Version, out result) && result < 8.0M)
      {
        LogManager.GetLogger("global.asax").Warn("User tried to access with IE < 9");
        Response.Write("This site requires Internet Explorer 8 or greater");
        Response.End();
      }
    }

    protected override void OnApplicationStarted()
    {
      base.OnApplicationStarted();

      GlobalConfiguration.Configure(config => WebApiConfig.Register(config, myKernel));

      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);
      Kcsar.Database.Model.Document.StorageRoot = Server.MapPath("~/content/auth/documents/");

      LogManager.GetLogger("global.asax").Info("Site has started");
    }

    protected override Ninject.IKernel CreateKernel()
    {
      return myKernel;
    }

    void Application_Error(object sender, EventArgs e)
    {
      int statusCode = 500;
      Exception exc = Server.GetLastError();

      var log = LogManager.GetLogger("global.asax");

      var httpException = exc as HttpException;
      //if (httpException != null)
      //{
      //  if (httpException.ErrorCode == -2147467259)
      //  {
      //    log.Info("Potentially dangerous request: " + Request.RawUrl, httpException);
      //    statusCode = 400;
      //  }
      //}
      //else
      //{
        log.Error(Request.RawUrl, exc);
      //}

      if (Request.RawUrl.ToLowerInvariant().StartsWith("/api/"))
      {
        Response.StatusCode = statusCode;
        Server.ClearError();
      }
    }
  }
}
