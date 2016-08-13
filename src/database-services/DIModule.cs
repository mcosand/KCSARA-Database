using System;
using Kcsar.Database.Model;
using Kcsara.Database.Services.Members;
using Kcsara.Database.Services.Training;
using Ninject.Modules;
using Sar.Services.Auth;

namespace Kcsara.Database.Services
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<Func<IKcsarContext>>().ToConstant((Func<IKcsarContext>)(() => new KcsarContext())).InSingletonScope();
      Bind<IUserInfoService>().To<UserInfoService>().InSingletonScope();
      Bind<IAuthorizationService>().To<AuthorizationService>().InSingletonScope();

      Bind<ITrainingRecordsService>().To<TrainingRecordsService>();
      Bind<IMembersService>().To<MembersService>();
      Bind<IUnitsService>().To<UnitsService>();
    }
  }
}
