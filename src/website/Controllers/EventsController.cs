/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Authorization;
  using Microsoft.AspNet.Mvc;
  using Models;
  using Services;

  [Authorize]
  public abstract class EventsController<RowType, ViewModelType> : BaseController
    where RowType : SarEventRow, new()
    where ViewModelType : EventSummary, new()
  {
    private readonly IEventsService<ViewModelType> service;

    public abstract string MenuGroup { get; }
    public abstract string EventTypeText { get; }

    public EventsController(Lazy<IEventsService<ViewModelType>> service, Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(db, log/*, settings*/)
    {
      this.service = service.Value;
    }

    //protected override void Initialize(RequestContext requestContext)
    //{
    //  base.Initialize(requestContext);
    //  menuGroup = ControllerContext.RouteData.Values["controller"].ToString().ToLowerInvariant().Replace("new", string.Empty);
    //}

    public ActionResult Index()
    {
      ViewBag.ActiveMenu = MenuGroup + "/Index";
      ViewData["showESAR"] = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["showKCESAR"]);

      return View();
    }

    public ActionResult List(string id = null)
    {
      int yearInt = DateTime.Now.Year;
      if (string.IsNullOrWhiteSpace(id))
      {
        yearInt = DateTime.Now.Year;
      }
      else if (int.TryParse(id, out yearInt))
      {
        if (yearInt < 0)
        {
          yearInt = DateTime.Now.Year + yearInt;
        }
      }
      else if (string.Equals(id, "all", StringComparison.OrdinalIgnoreCase))
      {
        yearInt = 0;
      }

      ViewBag.ActiveMenu = MenuGroup + "/List";
      ViewBag.EventTypeText = EventTypeText;
      ViewBag.EventRoute = MenuGroup;
      if (yearInt > 0)
      {
        ViewBag.Year = yearInt;
      }
      return View("EventList");
    }

    [HttpGet]
    [Route("api/[controller]/list/{year:int?}")]
    public EventList ApiList(int? year = null)
    {
      return service.List(year);
    }

    [HttpPost]
    [HandleException]
    [Route("api/[controller]")]
    public ViewModelType ApiCreate([FromBody] ViewModelType evt)
    {
      if (evt.Id != Guid.Empty) throw new StatusCodeException(System.Net.HttpStatusCode.BadRequest, "Id should be blank for POST requests");
      return service.Save(evt);
    }

    [HttpPut]
    [HandleException]
    [Route("api/[controller]")]
    public ViewModelType ApiUpdate([FromBody] ViewModelType evt)
    {
      if (evt.Id == Guid.Empty) throw new StatusCodeException(System.Net.HttpStatusCode.BadRequest, "Id is required");
      return service.Save(evt);
    }

    private IQueryable<SarEventRow> GetEventsOfType(string types)
    {
      IQueryable<SarEventRow> query = dbFactory.Value.Events;

      if (types.ToUpperInvariant() == "MISSIONS")
      {
        query = query.OfType<MissionRow>();
      }
      else if (types.ToUpperInvariant() == "TRAINING")
      {
        query = query.OfType<TrainingRow>();
      }

      return query;
    }

    [HttpGet]
    [Route("api/[controller]/years")]
    public IEnumerable<int> ApiYears()
    {
      var types = RouteData.Values["controller"].ToString();

      IQueryable<SarEventRow> query = GetEventsOfType(types);

      return query.Select(f => (int)SqlFunctions.DatePart("year", f.StartTime)).Distinct().Where(f => f > 1900).OrderByDescending(f => f);
    }

  }

  public class MissionsController : EventsController<MissionRow, Mission>
  {
    public override string EventTypeText { get { return "Mission"; } }
    public override string MenuGroup { get { return "Missions"; } }
    public MissionsController(Lazy<IEventsService<Mission>> service, Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(service, db, log/*, settings*/)
    {

    }
  }

  public class TrainingController : EventsController<TrainingRow, EventSummary>
  {
    public override string EventTypeText { get { return "Training"; } }
    public override string MenuGroup { get { return "Training"; } }

    public TrainingController(Lazy<IEventsService<EventSummary>> service, Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(service, db, log/*, settings*/)
    {

    }
  }
}