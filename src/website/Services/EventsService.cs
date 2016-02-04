/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Kcsar.Database.Model;
  using Kcsar.Database.Model.Events;
  using log4net;
  using Models;

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="ModelType"></typeparam>
  public interface IEventsService<ModelType> where ModelType : EventSummary, new()
  {
    ModelType Get(Guid id);

    EventList List(int? year);

    IEnumerable<EventSummary> Search(string query);


    ModelType Save(ModelType model);

    IEnumerable<LogEntry> Logs(Guid eventId);

    LogEntry SaveLog(Guid eventId, LogEntry entry);
    void DeleteLog(Guid eventId, Guid id);
    Roster Roster(Guid eventId);

    IEnumerable<ParticipantTimelineItem> ParticipantTimeline(Guid participantId);
    IEnumerable<ParticipantTimelineItem> MemberTimeline(Guid eventId, Guid memberId);
    Task<EventStatistics> Stats();
    object MemberEvents(Guid memberId, int? year = null);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class EventsService<RowType, ModelType> : IEventsService<ModelType>
    where RowType : SarEventRow, new()
    where ModelType : EventSummary, new()
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public EventsService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
    }

    public IEnumerable<LogEntry> Logs(Guid eventId)
    {
      using (var db = dbFactory())
      {
        return db.EventLogs.Where(f => f.EventId == eventId).OrderByDescending(f => f.Time)
          .Select(f => new LogEntry
          {
            Id = f.Id,
            Time = f.Time,
            Message = f.Data,
            LoggedBy = (f.Person == null) ? (f.Person.FirstName + " " + f.Person.LastName) : null
          })
          .ToList();
      }
    }

    public EventList List(int? year)
    {
      using (var db = dbFactory())
      {
        IQueryable<SarEventRow> query = db.Events.OfType<RowType>().AsNoTracking();

        if (year != null)
        {
          var dateStart = new DateTime(year.Value, 1, 1);
          var dateEnd = dateStart.AddYears(1);
          query = query.Where(f => f.StartTime >= dateStart && f.StartTime < dateEnd);
        }

        var result = (from e in query.SelectMany(f => f.Participants).DefaultIfEmpty()
                      group e by 1 into g
                      select new EventList
                      {
                        People = g.Distinct().Count(),
                        Hours = g.Sum(f => f.Hours),
                        Miles = g.Sum(f => f.Miles)
                      }).SingleOrDefault();

        var list = query
          .OrderBy(f => f.StartTime)
          .Select(f => new EventListItem
          {
            Id = f.Id,
            Number = f.StateNumber,
            Date = f.StartTime,
            Title = f.Title,
            People = f.Participants.Distinct().Count(),
            Hours = f.Participants.Sum(g => g.Hours),
            Miles = f.Participants.Sum(g => g.Miles)
          })
          .ToList();

        list.ForEach(f => f.Hours = (f.Hours == null) ? (double?)null : (Math.Round(f.Hours.Value * 4.0) / 4.0));
        result.Events = list;

        return result;
      }
    }

    public IEnumerable<EventSummary> Search(string query)
    {
      using (var db = dbFactory())
      {
        return db.Missions.Where(f => f.StateNumber.StartsWith(query) || f.Title.Contains(query) || f.Location.Contains(query))
                .OrderByDescending(f => f.StartTime)
                .AsEnumerable()
                .Select(f => new EventSummary
                {
                  Id = f.Id,
                  StateNumber = f.StateNumber,
                  Name = f.Title,
                  Location = f.Location,
                  Start = f.StartTime,
                  Stop = f.StopTime
                })
                .ToArray();
      }
    }

    public ModelType Get(Guid id)
    {
      using (var db = dbFactory())
      {
        var result = db.Events.OfType<RowType>().SingleOrDefault(f => f.Id == id);
        if (result == null) throw new NotFoundException();

        var model = new ModelType
        {
          Id = result.Id,
          Name = result.Title,
          Location = result.Location,
          StateNumber = result.StateNumber,
          Start = result.StartTime
        };
        return model;
      }
    }
    public ModelType Save(ModelType model)
    {
      using (var db = dbFactory())
      {
        RowType row = null;
        if (model.Id == Guid.Empty)
        {
          row = new RowType();
          db.Events.Add(row);
        }
        else
        {
          row = db.Events.OfType<RowType>().SingleOrDefault(f => f.Id == model.Id);
          if (row == null) throw new NotFoundException();
        }

        Dictionary<string, string> errors = new Dictionary<string, string>();

        if (model.Name != row.Title) { row.Title = model.Name; }
        if (string.IsNullOrWhiteSpace(row.Title)) { errors.Add("Name", "Required"); }

        if (model.Location != row.Location) { row.Location = model.Location; }
        if (string.IsNullOrWhiteSpace(row.Location)) { errors.Add("Location", "Required"); }


        if (model.StateNumber != row.StateNumber) { row.StateNumber = model.StateNumber; }

        if (model.Start != row.StartTime) { row.StartTime = model.Start; }
        if (model.Start < Kcsar.Database.Model.Utils.SarEpoch || model.Start > DateTime.Now.AddYears(1)) { errors.Add("Start", "Invalid Date"); }

        if (model.Stop != row.StopTime) { row.StopTime = model.Stop; }
        if (model.Stop != null && model.Stop < Kcsar.Database.Model.Utils.SarEpoch || model.Start > DateTime.Now.AddYears(1)) { errors.Add("Stop", "Invalid Date"); }

        InternalApiSaveProcessModel(model, row, errors);

        if (errors.Count > 0)
        {
          throw new ModelErrorsException(errors);
        }

        db.SaveChanges();

        model.Id = row.Id;
        return model;
      }
    }

    protected virtual void InternalApiSaveProcessModel(ModelType evt, RowType row, Dictionary<string, string> errors)
    {
    }

    public LogEntry SaveLog(Guid eventId, LogEntry entry)
    {
      Dictionary<string, string> errors = new Dictionary<string, string>();

      using (var db = dbFactory())
      {
        var theEvent = db.Events.SingleOrDefault(f => f.Id == eventId);
        if (theEvent == null)
        {
          throw new NotFoundException();
        }

        var row = theEvent.Log.SingleOrDefault(f => f.Id == entry.Id);
        if (row == null && entry.Id == Guid.Empty)
        {
          row = new EventLogRow()
          {
            EventId = eventId
          };
          db.Events.Single(f => f.Id == eventId).Log.Add(row);
        }

        if (row.EventId != eventId)
        {
          throw new StatusCodeException(System.Net.HttpStatusCode.BadRequest, "Can't change a log entry's event");
        }

        if (row.Time != entry.Time)
        {
          row.Time = entry.Time;
        }
        if (row.Time < theEvent.StartTime.AddMonths(-1) || row.Time > theEvent.StartTime.AddMonths(1))
        {
          errors.Add("Time", "Out of range. Must be within a month of the event.");
        }

        if (row.Data != entry.Message)
        {
          row.Data = entry.Message;
        }
        if (string.IsNullOrWhiteSpace(row.Data))
        {
          errors.Add("Message", "Required");
        }

        if (errors.Count > 0)
        {
          throw new ModelErrorsException(errors);
        }

        db.SaveChanges();
        entry.Id = row.Id;
        return entry;
      }
    }

    public void DeleteLog(Guid eventId, Guid id)
    {
      using (var db = dbFactory())
      {
        var eventRow = db.Events.OfType<RowType>().SingleOrDefault(f => f.Id == eventId);
        if (eventRow == null) throw new NotFoundException();

        var row = eventRow.Log.SingleOrDefault(f => f.Id == id);
        if (row == null) throw new NotFoundException();

        eventRow.Log.Remove(row);

        db.SaveChanges();
      }
    }

    public Roster Roster(Guid eventId)
    {
      var roster = new Roster();
      using (var db = dbFactory())
      {
        var list = db.Events.Where(f => f.Id == eventId)
          .SelectMany(f => f.Participants)
          .Select(f => new RosterEntry
          {
            Id = f.Id,
            MemberId = f.MemberId,
            FirstName = f.FirstName,
            LastName = f.LastName,
            Photo = f.MemberId == null ? null : f.Member.PhotoFile,
            Unit = f.EventUnitId == null ? null : new ParticipatingNameIdPair
            {
              Id = f.EventUnit.Id,
              PermanentId = f.EventUnit.MemberUnitId,
              Name = f.EventUnit.Name
            },
            Status = f.LastStatus,
            Hours = f.Hours,
            Miles = f.Miles
          })
          .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ThenBy(f => f.Id)
          .ToList();
        list.ForEach(f => f.Hours = f.Hours.HasValue ? Math.Round(f.Hours.Value * 4.0) / 4.0 : (double?)null);

        roster.Responders = list;

        roster.ResponderCount = roster.Responders.Distinct().Count();
      }
      return roster;
    }

    public object MemberEvents(Guid memberId, int? year = null)
    {
      using (var db = dbFactory())
      {
        var query = db.Events.OfType<RowType>().SelectMany(f => f.Participants).Where(f => f.MemberId == memberId).AsNoTracking();
        if (year != null)
        {
          var dateStart = new DateTime(year.Value, 1, 1);
          var dateEnd = dateStart.AddYears(1);
          query = query.Where(f => f.Event.StartTime >= dateStart && f.Event.StartTime < dateEnd);
        }

        var list = query.Select(f => new
        {
          Id = f.EventId,
          Number = f.Event.StateNumber,
          Date = f.Event.StartTime,
          Title = f.Event.Title,
          Unit = f.EventUnitId == null ? null : new ParticipatingNameIdPair { Id = f.EventUnitId.Value, Name = f.EventUnit.Name, PermanentId = f.EventUnit.MemberUnitId },          
          Hours = f.Hours,
          Miles = f.Miles
        })
        .OrderBy(f => f.Date)
        .ToList();

        //list.ForEach(f => f.Hours = (f.Hours == null) ? (double?)null : (Math.Round(f.Hours.Value * 4.0) / 4.0));
        return list;
      }
    }

    public IEnumerable<ParticipantTimelineItem> ParticipantTimeline(Guid participantId)
    {
      return Timeline(f => f.Id == participantId);
    }

    public IEnumerable<ParticipantTimelineItem> MemberTimeline(Guid eventId, Guid memberId)
    {
      return Timeline(f => f.MemberId == memberId && f.EventId == eventId);
    }

    private IEnumerable<ParticipantTimelineItem> Timeline(Expression<Func<EventParticipantRow, bool>> where)
    {
      using (var db = dbFactory())
      {

        return db.Events.SelectMany(f => f.Participants).Where(where)
          .SelectMany(f => f.Timeline)
          .Select(f => new ParticipantTimelineItem
          {
            Id = f.Id,
            Unit = f.EventUnitId == null ? null : new ParticipatingNameIdPair { Id = f.EventUnit.Id, Name = f.EventUnit.Name, PermanentId = f.EventUnit.MemberUnitId },
            Time = f.Time,
            Status = f.Status,
            Miles = f.Miles,
            Role = f.Role
          })
          .OrderByDescending(f => f.Time)
          .ToList();
      }
    }

    public async Task<EventStatistics> Stats()
    {
      using (var db = dbFactory())
      {
        var parts = await db.EventDashboardStatistics<EventStatisticsItem>(typeof(RowType).Name.Replace("Row", ""));

        return new EventStatistics
        {
          YearToDate = parts[0],
          Recent = parts[1],
          Average = parts[2]
        };
      }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  public class MissionsService : EventsService<MissionRow, Mission>
  {
    public MissionsService(Func<IKcsarContext> dbFactory, ILog log) : base(dbFactory, log)
    { }

    protected override void InternalApiSaveProcessModel(Mission evt, MissionRow row, Dictionary<string, string> errors)
    {
      base.InternalApiSaveProcessModel(evt, row, errors);
      //var allowedTypes = string
      //var invalidTypes = evt.Type.Select(f => f.ToLowerInvariant()).Where()

      var types = string.Join(",", (evt.Type ?? new string[0]).Select(f => f.ToLowerInvariant()));
      if (types != row.MissionType) { row.MissionType = types; }
    }
  }
}
