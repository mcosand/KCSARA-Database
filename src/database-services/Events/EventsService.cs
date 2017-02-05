using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Search;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IEventsService
  {
    Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter);

    Task<IList<EventSearchResult>> SearchMissionsAsync(string query);
  }

  public abstract class EventsService<TRow, TRoster> : IEventsService
    where TRow : class, DB.IRosterEvent<TRow, TRoster>
    where TRoster : class, DB.IRosterEntry<TRow, TRoster>
  {
    protected readonly Func<DB.IKcsarContext> _dbFactory;
    protected readonly IAuthorizationService _authz;
    protected readonly IHost _env;

    protected readonly string _eventType;
    protected readonly Func<DB.IKcsarContext, IDbSet<TRow>> _rootTableFunc;

    /// <summary></summary>
    /// <param name="dbFactory"></param>
    /// <param name="authSvc"></param>
    /// <param name="host"></param>
    public EventsService(string eventType, Func<DB.IKcsarContext> dbFactory, Func<DB.IKcsarContext, IDbSet<TRow>> rootTableFunc, IAuthorizationService authSvc, IHost env)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _env = env;

      _eventType = eventType;
      _rootTableFunc = rootTableFunc;
    }

    protected virtual Expression<Func<TRow, GroupEventAttendance>> EventListProjection
    {
      get
      {
        return f => new GroupEventAttendance
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
        }; ;
      }
    }

    public async Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter)
    {
      filter = filter ?? (f => true);

      using (var db = _dbFactory())
      {
        var list = await _rootTableFunc(db)
          .Select(EventListProjection)
          .Where(filter)
          .OrderByDescending(f => f.Event.Start)
          .ToListAsync();

        return new ListPermissionWrapper<GroupEventAttendance>
        {
          C = _authz.Authorize(null, $"Create:{_eventType}"),
          Items = list.Select(f => new ItemPermissionWrapper<GroupEventAttendance>
          {
            Item = f,
            D = _authz.Authorize(f.Event.Id, $"Delete:{_eventType}"),
            U = _authz.Authorize(f.Event.Id, $"Update:{_eventType}")
          })
        };
      }
    }
    

    public async Task<IList<EventSearchResult>> SearchMissionsAsync(string query)
    {
      var now = DateTime.Now;
      var last12Months = now.AddMonths(-12);

      using (var db = _dbFactory())
      {
        var summaries = await MissionSummariesAsync(db.Missions.Where(f => f.StateNumber.StartsWith(query) || f.Title.Contains(query) || f.Location.Contains(query)));
        var list = summaries.Select((m, i) => new EventSearchResult
        {
          Score = ((m.Start > last12Months) ? 500 : 200) - i,
          Summary = m,
          Type = SearchResultType.Mission
        }).ToList();

        return list;
      }
    }

    private async Task<IList<EventSummary>> MissionSummariesAsync(IQueryable<DB.Mission> queryable)
    {
      return await queryable
        .OrderByDescending(f => f.StartTime)
        .Select(f => new EventSummary
        {
          Id = f.Id,
          StateNumber = f.StateNumber,
          Name = f.Title,
          Location = f.Location,
          Start = f.StartTime,
          Stop = f.StopTime
        }).ToListAsync();
    }
  }
}
