/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using Kcsar.Database.Data;
  using Kcsar.Database.Data.Events;
  using Kcsara.Database.Model.Events;
  using log4net;
  using Ninject;

  public static class DIConfig
  {
    public static void StandardConfig(IKernel kernel)
    {
      kernel.Bind<Func<IKcsarContext>>().ToConstant((Func<IKcsarContext>)(() =>
      {
        var db = new KcsarContext();
        db.Database.Log = kernel.Get<ILog>().Debug;
        return db;
      }));
      kernel.Bind<ISarEventsService<Mission>>().To<SarEventsService<Mission, MissionRow>>().InSingletonScope();
      kernel.Bind<ISarEventsService<Training>>().To<SarEventsService<Training, TrainingRow>>().InSingletonScope();
      kernel.Bind<IUnitsService>().To<UnitsService>().InSingletonScope();
      kernel.Bind<IMembersService>().To<MembersService>().InSingletonScope();
      kernel.Bind<ITrainingService>().To<TrainingService>().InSingletonScope();
    }
  }
}
