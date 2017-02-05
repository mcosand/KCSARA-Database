using System;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public class MissionsService : EventsService<DB.Mission, DB.MissionRoster>, IMissionsService
  {
    /// <summary></summary>
    public MissionsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
      : base("Mission", dbFactory, db => db.Missions, authSvc, host)
    {
    }
  }

  public interface IMissionsService : IEventsService
  {
  }
}
