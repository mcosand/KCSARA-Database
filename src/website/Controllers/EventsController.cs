/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Routing;
  using Kcsar.Database.Model;

  [Authorize]
  public abstract class EventsController : BaseController
  {
    string menuGroup;
    public abstract string EventTypeText { get; }

    public EventsController(IKcsarContext db, IAppSettings settings) : base(db, settings)
    {
    }

    protected override void Initialize(RequestContext requestContext)
    {
      base.Initialize(requestContext);
      menuGroup = ControllerContext.RouteData.Values["controller"].ToString().ToLowerInvariant().Replace("new", string.Empty);
    }

    public ActionResult Index()
    {
      ViewBag.ActiveMenu = menuGroup + "/Index";
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

      ViewBag.ActiveMenu = menuGroup + "/List";
      ViewBag.EventTypeText = EventTypeText;
      ViewBag.EventRoute = menuGroup;
      if (yearInt > 0)
      {
        ViewBag.Year = yearInt;
      }
      return View("EventList");
    }
  }

  public class NewMissionsController : EventsController
  {
    public override string EventTypeText { get { return "Mission"; } }
    public NewMissionsController(IKcsarContext db, IAppSettings settings) : base(db, settings)
    {

    }
  }

  public class NewTrainingController : EventsController
  {
    public override string EventTypeText { get { return "Training"; } }

    public NewTrainingController(IKcsarContext db, IAppSettings settings) : base(db, settings)
    {

    }
  }
}