/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  using System.Web.Http;
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
  }
}