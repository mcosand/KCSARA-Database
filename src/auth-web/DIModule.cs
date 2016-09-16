using System;
using System.Linq;
using IdentityServer3.Core.Services;
using Ninject;
using Ninject.Modules;
using Sar.Auth.Data;
using Sar.Auth.Services;
using Sar.Services;
using Sar.Services.Auth;
using Serilog;

namespace Sar.Auth
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      var logConfig = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\auth-log-{Date}.txt");
      Log.Logger = logConfig.CreateLogger();
      Bind<ILogger>().ToConstant(Log.Logger);

      Bind<SarUserService>().ToSelf();
      Bind<IClientStore>().To<SarClientStore>();
      Bind<Func<IAuthDbContext>>().ToMethod(ctx => () => new AuthDbContext());
      Bind<ISendEmailService>().To<DefaultSendMessageService>().InSingletonScope();
      Bind<IRolesService>().To<RolesService>().InSingletonScope();

      var config = Kernel.Get<IHost>();

      if (!Kernel.GetBindings(typeof(IMemberInfoService)).Any())
      {
        Kernel.Bind<IMemberInfoService>().To<LocalMemberInfoService>();
      }
    }
  }
}
