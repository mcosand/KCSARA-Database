[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Kcsara.Database.Web.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(Kcsara.Database.Web.NinjectWebCommon), "Stop")]

namespace Kcsara.Database.Web
{
  using System;
  using System.IO;
  using System.Security.Principal;
  using System.Threading;
  using System.Web;
  using Database.Web.Services;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.Web.Infrastructure.DynamicModuleHelper;

  using Ninject;
  using Ninject.Web.Common;
  using Sar;
  using Sar.Auth.Data;
  using Sar.Database.Web;
  using Sar.WebApi;

  public static class NinjectWebCommon
  {
    private static readonly Bootstrapper bootstrapper = new Bootstrapper();

    /// <summary>
    /// Starts the application
    /// </summary>
    public static void Start()
    {
      DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
      DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
      bootstrapper.Initialize(CreateKernel);
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public static void Stop()
    {
      bootstrapper.ShutDown();
    }

    /// <summary>
    /// Creates the kernel that will manage your application.
    /// </summary>
    /// <returns>The created kernel.</returns>
    private static IKernel CreateKernel()
    {
      var kernel = new StandardKernel();
      try
      {
        kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
        kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

        RegisterServices(kernel);
      }
      catch
      {
        kernel.Dispose();
        throw;
      }
      Startup.kernel = kernel;
      MvcApplication.kernel = kernel;
      return kernel;
    }

    /// <summary>
    /// Load your modules or register your services here!
    /// </summary>
    /// <param name="kernel">The kernel.</param>
    private static void RegisterServices(IKernel kernel)
    {
      AuthDbContext.SetInitializer();
      KcsarContext.SetInitializer();

      var logConfigPath = new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "log4net.config"));
      if (File.Exists(logConfigPath.FullName))
      {
        log4net.Config.XmlConfigurator.ConfigureAndWatch(logConfigPath);
      }
      else
      {
        log4net.Config.XmlConfigurator.Configure();
      }

      kernel.Bind<IKcsarContext>().To<KcsarContext>();
      kernel.Bind<ILog>().ToMethod(context => LogManager.GetLogger("Default"));
      kernel.Bind<Services.IAuthService>().To<Services.AuthService>();
      kernel.Bind<IPrincipal>().ToMethod(f => Thread.CurrentPrincipal);
      kernel.Bind<Func<IPrincipal>>().ToConstant((Func<IPrincipal>)(() => Thread.CurrentPrincipal));
      kernel.Bind<IAppSettings>().To<AppSettings>();
      kernel.Bind<IReportsService>().To<ReportsService>();

      var host = new SystemWebHost();
      kernel.Bind<IHost>().ToConstant(host);
      kernel.Bind<IWebApiHost>().ToConstant(host);
      kernel.Load(new Services.DIModule());

      kernel.Load(new Sar.Database.Web.Auth.DIModule());
      kernel.Load(new Kcsara.Database.Api.DIModule());
      kernel.Load(new Sar.Database.Services.DIModule());
    }
  }
}
