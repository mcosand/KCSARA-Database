using Ninject.Modules;

namespace Sar.Database.Services
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IAuthorizationService>().To<AuthorizationService>().InSingletonScope();

      Bind<ITrainingRecordsService>().To<TrainingRecordsService>();
      Bind<IMembersService>().To<MembersService>();
      Bind<IUnitsService>().To<UnitsService>();
    }
  }
}
