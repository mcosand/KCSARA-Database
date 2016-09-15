using Ninject.Modules;
using Sar.Services;
using Sar.Services.Auth;

namespace Kcsara.Database.Api
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IHost>().ToMethod(context => new OwinHost());
      Bind<IAuthenticatedHost>().ToMethod(context => new OwinHost());
    }
  }
}
