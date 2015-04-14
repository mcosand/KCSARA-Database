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
  using Kcsara.Database.Model.Events;
  using log4net;

  public interface ISarEventsService
  {
    List<SarEventSummary> List(Expression<Func<SarEventSummary, bool>> filter = null, int? maxCount = null);
    List<int> ListYears();
    List<ParticipationSummary> ParticipantList(Expression<Func<ParticipationSummary, bool>> filter = null);
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
          Number = row.StateNumber,
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
  }
}
