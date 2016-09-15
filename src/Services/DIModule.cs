using System;
using Kcsar.Database.Model;
using Ninject.Modules;

namespace Kcsara.Database.Web.Services
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<Func<IKcsarContext>>().ToConstant((Func<IKcsarContext>)(() => new KcsarContext())).InSingletonScope();
      Bind<IUserInfoService>().To<UserInfoService>().InSingletonScope();
      Bind<IAuthorizationService>().To<AuthorizationService>().InSingletonScope();
    }
  }
}
