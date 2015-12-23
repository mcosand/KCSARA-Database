/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using System.Web.Http;
  using Kcsar.Database.Model;
  using Kcsara.Database.Services;
  using log4net;

  [CamelCaseControllerConfig]
  public class EventsController : BaseApiController
  {
    public EventsController(IKcsarContext db, IAuthService auth, ILog log) : base(db, auth, log)
    {

    }

    [HttpGet]
    public EventList List(int? id = null, string types = "")
    {
      IQueryable<SarEvent> query = GetEventsOfType(types);

      if (id != null)
      {
        var dateStart = new DateTime(id.Value, 1, 1);
        var dateEnd = dateStart.AddYears(1);
        query = query.Where(f => f.StartTime >= dateStart && f.StartTime < dateEnd);
      }

      var result = (from e in query.SelectMany(f => f.Roster).DefaultIfEmpty()
                    group e by 1 into g
                    select new EventList
                    {
                      People = g.DefaultIfEmpty().Select(f => (Guid?)f.Person.Id).Where(f => f != null).Distinct().Count(),
                      Hours = Math.Round(g.Sum(f => SqlFunctions.DateDiff("minute", f.TimeIn, f.TimeOut) / 15.0) ?? 0.0) / 4.0,
                      Miles = g.Sum(f => f.Miles)
                    }).SingleOrDefault();

      result.Events = query
        .OrderBy(f => f.StartTime)
        .Select(f => new
        {
          Id = f.Id,
          Number = f.StateNumber,
          Date = f.StartTime,
          Title = f.Title,
          People = f.Roster.DefaultIfEmpty().Select(g => (Guid?)g.Person.Id).Where(g => g != null).Distinct().Count(),
          Hours = Math.Round(f.Roster.DefaultIfEmpty().Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 15.0) ?? 0.0) / 4.0,
          Miles = f.Roster.DefaultIfEmpty().Sum(g => g.Miles)
        });

      return result;
    }

    private IQueryable<SarEvent> GetEventsOfType(string types)
    {
      IQueryable<SarEvent> query = db.Events;

      if (types == "missions")
      {
        query = query.OfType<Mission>();
      }
      else if (types == "training")
      {
        query = query.OfType<Training>();
      }

      return query;
    }

    [HttpGet]
    [Authorize]
    public IEnumerable<int> Years(string types = "")
    {
      IQueryable<SarEvent> query = GetEventsOfType(types);

      return query.Select(f => (int)SqlFunctions.DatePart("year", f.StartTime)).Distinct().Where(f => f > 1900).OrderByDescending(f => f);
    }
  }
}