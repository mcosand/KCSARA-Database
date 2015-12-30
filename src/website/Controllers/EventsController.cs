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
  using ViewModels;

  [Authorize]
  public abstract class EventsController : BaseController
  {
    public abstract string MenuGroup { get; }
    public abstract string EventTypeText { get; }

    public EventsController(Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(db, log/*, settings*/)
    {
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
    [Route("api/[controller]/list/{id:int?}")]
    public EventList ApiList(int? id = null)
    {
      var types = RouteData.Values["controller"].ToString();

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
      IQueryable<SarEvent> query = dbFactory.Value.Events;

      if (types.ToUpperInvariant() == "MISSIONS")
      {
        query = query.OfType<Mission>();
      }
      else if (types.ToUpperInvariant() == "TRAINING")
      {
        query = query.OfType<Training>();
      }

      return query;
    }

    [HttpGet]
    [Route("api/[controller]/years")]
    public IEnumerable<int> ApiYears()
    {
      var types = RouteData.Values["controller"].ToString();

      IQueryable<SarEvent> query = GetEventsOfType(types);

      return query.Select(f => (int)SqlFunctions.DatePart("year", f.StartTime)).Distinct().Where(f => f > 1900).OrderByDescending(f => f);
    }

  }

  public class MissionsController : EventsController
  {
    public override string EventTypeText { get { return "Mission"; } }
    public override string MenuGroup { get { return "Missions";  } }
    public MissionsController(Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(db, log/*, settings*/)
    {

    }
  }

  public class TrainingController : EventsController
  {
    public override string EventTypeText { get { return "Training"; } }
    public override string MenuGroup { get { return "Training";  } }

    public TrainingController(Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(db, log/*, settings*/)
    {

    }
  }
}