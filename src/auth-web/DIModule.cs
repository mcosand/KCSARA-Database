using System;
using System.Linq;
using IdentityServer3.Core.Services;
using Ninject;
using Ninject.Modules;
using Sar.Auth.Data;
using Sar.Database.Services;
using Sar.Database.Web.Auth.Services;
using Serilog;

namespace Sar.Database.Web.Auth
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

      var scopeStore = SarScopeStore.Create(() => new AuthDbContext());

      Bind<SarUserService>().ToSelf();
      Bind<IClientStore>().To<SarClientStore>();
      Bind<IScopeStore>().ToConstant(scopeStore);
      Bind<Func<IAuthDbContext>>().ToMethod(ctx => () => new AuthDbContext());
      Bind<ISendEmailService>().To<DefaultSendMessageService>().InSingletonScope();
      Bind<IRolesService>().To<RolesService>().InSingletonScope();
      Bind<IUsersService>().To<ApiUsersService>().InSingletonScope();
      Bind<EntityFrameworkTokenHandleStore>().ToSelf();

      var config = Kernel.Get<IHost>();

      if (!Kernel.GetBindings(typeof(IMemberInfoService)).Any())
      {
        Kernel.Bind<IMemberInfoService>().To<LocalMemberInfoService>();
      }
    }
  }
}
