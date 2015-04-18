﻿/*
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
  using log4net;

  public interface ISarEventsService
  {
    List<SarEventSummary> List(Expression<Func<SarEventSummary, bool>> filter = null, int? maxCount = null);
    List<int> ListYears();
    List<ParticipationSummary> ParticipantList(Expression<Func<ParticipationSummary, bool>> filter = null);
    EventOverview GetOverview(Guid id);
    List<RosterEntry> ListRoster(Guid eventId);
    List<TimelineEntry> ListTimeline(Guid eventId);
  }

  public interface ISarEventsService<T> : ISarEventsService where T : SarEvent
  { }

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
    public EventOverview GetOverview(Guid id)
    {
      using (var db = this.dbFactory())
      {
        return db.Events.Where(f => f.Id == id).Select(f => new EventOverview
        {
          Id = f.Id,
          Title = f.Title,
          Start = f.StartTime,
          IdNumber = f.StateNumber
        }).FirstOrDefault();
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