using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CsvHelper;
using Sar.Database.Data;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Services.Training;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public class TrainingsService : ITrainingsService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    /// <summary></summary>
    public TrainingsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    public async Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter = null)
    {
      filter = filter ?? (f => true);
      using (var db = _dbFactory())
      {
        var list = await db.Trainings
          .Select(f => new GroupEventAttendance
          {
            Event = new EventSummary
            {
              Id = f.Id,
              Name = f.Title,
              Location = f.Location,
              Start = f.StartTime,
              StateNumber = f.StateNumber,
              Stop = f.StopTime
            },
            Persons = f.Roster.Select(g => g.PersonId).Distinct().Count(),
            Miles = f.Roster.Sum(g => g.Miles) ?? 0,
            Hours = f.Roster.Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 60.0) ?? 0.0
          })
        .Where(filter)
        .OrderByDescending(f => f.Event.Start)
        .ToListAsync();

        return new ListPermissionWrapper<GroupEventAttendance>
        {
          C = _authz.Authorize(null, "Create:Training"),
          Items = list.Select(f => new ItemPermissionWrapper<GroupEventAttendance>
          {
            Item = f,
            D = _authz.Authorize(f.Event.Id, "Delete:Training"),
            U = _authz.Authorize(f.Event.Id, "Update:Training")
          })
        };
      }
    }
  }

  public interface ITrainingsService
  {
    Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter = null);
  }
}
