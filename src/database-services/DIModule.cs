using Ninject;
using Ninject.Modules;
using Sar.Database.Api.Extensions;

namespace Sar.Database.Services
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IAuthorizationService>().To<AuthorizationService>().InSingletonScope();

      Bind<IAnimalsService>().To<AnimalsService>();
      Bind<ITrainingsService>().To<TrainingsService>();
      Bind<ITrainingRecordsService>().To<TrainingRecordsService>();
      Bind<ITrainingCoursesService>().To<TrainingCoursesService>();
      Bind<IMembersService>().To<MembersService>();
      Bind<IUnitsService>().To<UnitsService>();
      Bind<IEventsService>().To<EventsService>();
      Bind<IExtensionProvider>().To<ExtensionProvider>().InSingletonScope();
      Kernel.Get<IExtensionProvider>().Initialize();
    }
  }
}
