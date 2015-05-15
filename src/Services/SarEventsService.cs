/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using Kcsar.Database.Data.Events;
  using Kcsara.Database.Model;
  using Kcsara.Database.Model.Events;
  using Kcsara.Database.Services.Hubs;
  using log4net;
  using Microsoft.AspNet.SignalR;

  public interface ISarEventsService
  {
    List<SarEventSummary> List(Expression<Func<SarEventSummary, bool>> filter = null, int? maxCount = null);
    List<int> ListYears();
    List<ParticipationSummary> ParticipantList(Expression<Func<ParticipationSummary, bool>> filter = null);
    List<RosterEntry> ListRoster(Guid eventId);
    List<TimelineEntry> ListTimeline(Guid eventId);
    SubmitResult Delete(Guid id);
  }

  public interface ISarEventsService<T> : ISarEventsService where T : SarEvent
  {
    T GetOverview(Guid id);
    SubmitResult<T> Update(T model);
  }

  public class SarEventsService<T, D> : BaseDataService, ISarEventsService<T>
    where T : SarEvent, new()
    where D : SarEventRow, new()
  {
    /// <summary>Default Constructor</summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public SarEventsService(Func<IKcsarContext> dbFactory, ILog log)
      : base(dbFactory, log)
    {
    }

    protected Func<D, T> toModel = row =>
      new T
      {
        Id = row.Id,
        IdNumber = row.StateNumber,
        Title = row.Title,
        Start = row.StartTime,
        Stop = row.StopTime,
        Jurisdiction = row.County,
        Location = row.Location,
        MissionType = (row.MissionType == null ? new string[0] : row.MissionType.Split(',')).ToList()
      };

    protected virtual Expression<Func<D, SarEventSummary>> GetSummaryProjection()
    {
      return row => new SarEventSummary
        {
          Id = row.Id,
          IdNumber = row.StateNumber,
          Title = row.Title,
          Start = row.StartTime,
          Participants = row.Participants.Count,
          Hours = row.Roster.Sum(f => f.Hours),
          Miles = row.Roster.Sum(f => f.Miles)
        };
    }

    protected Expression<Func<ParticipantRow, ParticipationSummary>> toDomainParticipantSummary = row =>
      new ParticipationSummary
      {
        Id = row.Id,
        EventId = row.EventId,
        Title = row.Event.Title,
        Number = row.Event.StateNumber,
        Start = row.Event.StartTime,
        Hours = row.Rosters.Sum(f => f.Hours),
        Miles = row.Rosters.Sum(f => f.Miles),
        MemberId = row.MemberId,
        Name = row.Lastname + ", " + row.Firstname
      };

    /// <summary>Get a list of events matching the given criteria</summary>
    /// <param name="filter"></param>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public List<SarEventSummary> List(Expression<Func<SarEventSummary, bool>> filter = null, int? maxCount = null)
    {
      using (var db = this.dbFactory())
      {
        var query = db.Events.OfType<D>().Select(GetSummaryProjection());
        if (filter != null)
        {
          query = query.Where(filter);
        }
        query = query.OrderByDescending(f => f.Start);
        if (maxCount.HasValue)
        {
          query = query.Take(maxCount.Value);
        }
        return query.ToList();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public SubmitResult<T> Update(T model)
    {
      var result = new SubmitResult<T>();


      using (var db = this.dbFactory())
      {
        D row = null;
        if (model.Id != Guid.Empty)
        {
          row = db.Events.OfType<D>().Single(f => f.Id == model.Id);
        }
        else
        {
          row = new D();
          db.Events.Add(row);
        }

        row.Title = model.Title;
        row.Location = model.Location;
        row.MissionType = model.MissionType == null ? null : string.Join(",", model.MissionType.OrderBy(f => f));
        row.StartTime = model.Start;
        row.StopTime = model.Stop;
        row.County = model.Jurisdiction;

        if (result.Errors.Count == 0)
        {
          db.SaveChanges();
          result.Data = toModel(row);
          GlobalHost.ConnectionManager.GetHubContext<AppHub>().Clients.All.eventUpdated(result.Data);
        }
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SubmitResult Delete(Guid id)
    {
      var result = new SubmitResult();
      using (var db = this.dbFactory())
      {
        var victim = db.Events.Where(f => f.Id == id).Select(f => new { Row = f, RosterCount = f.Participants.Count }).SingleOrDefault();
        if (victim == null)
        {
          result.Errors.Add(new SubmitError("Event not found"));
        }
        else if (victim.RosterCount > 0)
        {
          result.Errors.Add(new SubmitError(string.Format("Event still has {0} participants", victim.RosterCount)));
        }
        
        if (result.Errors.Count == 0)
        {
          db.Events.Remove(victim.Row);
          db.SaveChanges();
        }
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public List<ParticipationSummary> ParticipantList(Expression<Func<ParticipationSummary, bool>> filter = null)
    {
      using (var db = this.dbFactory())
      {
        var query = db.Events.OfType<D>().SelectMany(f => f.Participants).Select(toDomainParticipantSummary);
        if (filter != null)
        {
          query = query.Where(filter);
        }

        query = query.OrderByDescending(f => f.Start).ThenBy(f => f.Name);
        return query.ToList();
      }
    }

    /// <summary>Gets a list of years containing event data.</summary>
    /// <returns></returns>
    public List<int> ListYears()
    {
      int eon = 1940;

      using (var db = this.dbFactory())
      {
        return db.Events.OfType<D>().Select(f => f.StartTime.Year).Where(f => f > eon).Distinct().OrderBy(f => f).ToList();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public T GetOverview(Guid id)
    {
      using (var db = this.dbFactory())
      {
        return db.Events.OfType<D>().Where(f => f.Id == id).Select(toModel).FirstOrDefault();
      }
    }


    public List<RosterEntry> ListRoster(Guid eventId)
    {
      using (var db = this.dbFactory())
      {
        return db.Events.Where(f => f.Id == eventId).SelectMany(f => f.Roster)
          .Select(f => new RosterEntry
          {
            Id = f.Id,
            Participant = new Participant
            {
              Id = f.Participant.Id,
              Name = f.Participant.Lastname + ", " + f.Participant.Firstname,
              IdNumber = f.Participant.WorkerNumber,
              MemberId = f.Participant.MemberId
            },
            Unit = new NameIdPair
            {
              Id = f.UnitId,
              Name = f.Unit.Nickname
            },
            Hours = f.Hours,
            Miles = f.Miles
          })
          .OrderBy(f => f.Unit.Name)
          .ThenBy(f => f.Participant.Name)
          .ToList();
      }
    }

    public List<TimelineEntry> ListTimeline(Guid eventId)
    {
      using (var db = this.dbFactory())
      {
        return db.Events.Include("Timeline.Participant").Where(f => f.Id == eventId)
          .SelectMany(f => f.Timeline)
          .AsEnumerable()
          .Select(f => new TimelineEntry
          {
            Id = f.Id,
            Time = f.Time,
            Markdown = f.ToMarkdown(),
            Type = f.GetType().Name.Replace("Row", string.Empty).Split('_')[0]
          })
          .OrderByDescending(f => f.Time)
          .ToList();
      }
    }
  }
}
