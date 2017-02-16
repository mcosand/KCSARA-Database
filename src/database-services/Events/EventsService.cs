using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Events;
using Sar.Database.Model.Search;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public abstract class EventsService<TRow, TRoster> : IEventsService
    where TRow : class, DB.IRosterEvent<TRow, TRoster>
    where TRoster : class, DB.IRosterEntry<TRow, TRoster>
  {
    protected readonly Func<DB.IKcsarContext> _dbFactory;
    protected readonly IAuthorizationService _authz;
    protected readonly IHost _env;

    protected readonly string _eventType;
    protected readonly Func<DB.IKcsarContext, IDbSet<TRow>> _rootTableFunc;

    private static MemoryCache statsCache = new MemoryCache("eventsStats");
    private static CacheItemPolicy statsPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };

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
          Id = f.Id,
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

    public async Task<ListPermissionWrapper<GroupEventAttendance>> ListForYear(int year)
    {
      string cacheKey = GetType().Name + ":year" + year;
      var list = (ListPermissionWrapper<GroupEventAttendance>)statsCache.Get(cacheKey);
      if (list == null)
      {
        var newYears = new DateTime(year, 1, 1);
        var nextYear = new DateTime(year + 1, 1, 1);
        list = await List(f => f.Event.Start >= newYears && f.Event.Start < nextYear);
        statsCache.Set(cacheKey, list, statsPolicy);
      }
      return list;
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

    public async Task Delete(Guid eventId)
    {
      using (var db = _dbFactory())
      {
        var table = _rootTableFunc(db);
        var evt = await table.FirstOrDefaultAsync(f => f.Id == eventId);

        if (evt == null) throw new NotFoundException("not found", _eventType, eventId.ToString());
        table.Remove(evt);

        await db.SaveChangesAsync();
        InvalidateCache(evt.StartTime.Year);
      }
    }

    public async Task<List<EventYearSummary>> ListYears()
    {
      string cacheKey = GetType().Name + ":years";
      var list = (List<EventYearSummary>)statsCache.Get(cacheKey);
      if (list == null)
      {
        using (var db = _dbFactory())
        {
          list = await _rootTableFunc(db)
            .GroupBy(f => SqlFunctions.DatePart("year", f.StartTime))
            .Select(f => new { k = f.Key, c = f.Count(), r = f.SelectMany(g => g.Roster) })
            .Select(f => new EventYearSummary
            {
              Year = f.k ?? 0,
              Count = f.c,
              Participants = f.r.Select(g => g.PersonId).Distinct().Count(),
              Hours = f.r.Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 60.0) ?? 0.0,
              Miles = f.r.Sum(g => g.Miles) ?? 0
            })
            .OrderByDescending(f => f.Year)
            .ToListAsync();

          list.Insert(0, new EventYearSummary
          {
            Year = 0,
            Count = list.Sum(f => f.Count),
            Participants = await _rootTableFunc(db).SelectMany(f => f.Roster).Select(f => f.PersonId).Distinct().CountAsync(),
            Hours = list.Sum(f => f.Hours),
            Miles = list.Sum(f => f.Miles)
          });

          statsCache.Add(cacheKey, list, statsPolicy);
        }
      }
      return list;
    }

    public void InvalidateCache(int year)
    {
      statsCache.Remove(GetType().Name + ":years");
      statsCache.Remove(GetType().Name + ":year" + year);
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

  public interface IEventsService
  {
    Task<ListPermissionWrapper<GroupEventAttendance>> List(Expression<Func<GroupEventAttendance, bool>> filter);
    Task<ListPermissionWrapper<GroupEventAttendance>> ListForYear(int year);
    Task Delete(Guid eventId);

    Task<List<EventYearSummary>> ListYears();

    Task<IList<EventSearchResult>> SearchMissionsAsync(string query);

    void InvalidateCache(int year);
  }
}
