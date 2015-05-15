/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  using System.Web.Http;
  using Kcsara.Database.Model;
  using Kcsara.Database.Model.Events;
  using Kcsara.Database.Services;
  using log4net;

  public abstract class SarEventsController<T> : NoDataBaseApiController where T : SarEvent, new()
  {
    protected readonly ISarEventsService<T> eventService;

    public SarEventsController(ISarEventsService<T> eventService, IAuthService auth, ILog log)
      : base(auth, log)
    {
      this.eventService = eventService;
    }

    [HttpGet]
    [Authorize]
    [HttpPost]
    [Authorize]
    public SubmitResult<T> Update(T model)
    {
      return eventService.Update(model);
    }

    [HttpPost]
    [Authorize]
    public SubmitResult Delete(Guid id)
    {
      return eventService.Delete(id);
    }

    [HttpGet]
    [Authorize]
    public List<SarEventSummary> List(int? id = null, int? size = null)
    {
      Expression<Func<SarEventSummary, bool>> filter = null;
      if (id.HasValue)
      {
        DateTime start = new DateTime(id.Value, 1, 1);
        DateTime stop = new DateTime(id.Value + 1, 1, 1);
        filter = row => row.Start > start && row.Start < stop;
      }
      return eventService.List(filter, size);
    }

    [HttpGet]
    [Authorize]
    public List<int> ListYears()
    {
      return eventService.ListYears();
    }

    [HttpGet]
    [Authorize(Roles = "cdb.users")]
    public T Overview(Guid id)
    {
      return eventService.GetOverview(id);
    }

    [HttpGet]
    [Authorize(Roles = "cdb.users")]
    public List<RosterEntry> Roster(Guid id)
    {
      return eventService.ListRoster(id);
    }

    [HttpGet]
    [Authorize(Roles = "cdb.users")]
    public List<TimelineEntry> Timeline(Guid id)
    {
      return eventService.ListTimeline(id);
    }
  }
}