using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sar.Database.Model;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public class TrainingsService : EventsService<DB.Training, DB.TrainingRoster>, ITrainingsService
  {

    /// <summary></summary>
    public TrainingsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
      : base("Training", dbFactory, db => db.Trainings, authSvc, host)
    {
    }
  }

  public interface ITrainingsService : IEventsService
  {
    Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter = null);
  }
}
