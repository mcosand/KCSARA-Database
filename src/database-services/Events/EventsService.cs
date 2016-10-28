using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Search;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IEventsService
  {
    Task<IList<EventSearchResult>> SearchMissionsAsync(string query);
  }

  public class EventsService : IEventsService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _env;

    /// <summary></summary>
    /// <param name="dbFactory"></param>
    /// <param name="authSvc"></param>
    /// <param name="host"></param>
    public EventsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost env)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _env = env;
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
