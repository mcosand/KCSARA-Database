/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using Kcsar.Database.Model;
  using Microsoft.AspNet.Authorization;
  using Microsoft.AspNet.Mvc;
  using Microsoft.Extensions.Logging;
  using Services;

  [Authorize]
  public class UnitsController : BaseController
  {
    private readonly IUnitsService service;
    
    public UnitsController(Lazy<IUnitsService> service, Lazy<IKcsarContext> db, ILogger<UnitsController> log)
      : base(db, log)
    {
      this.service = service.Value;
    }

    [Route("/Units")]
    public ActionResult Index()
    {
      ViewBag.ActiveMenu = "Units";
      return View(service.List());
    }

    [Route("/Units/{unitId}")]
    public ActionResult Detail(Guid unitId)
    {
      ViewBag.ActiveMenu = "Units";
      return View(service.GetUnit(unitId));
    }

    [Route("/Units/{unitId}/Roster")]
    public ActionResult Roster(Guid unitId)
    {
      ViewBag.ActiveMenu = "Units";
      return View(service.GetUnit(unitId));
    }

    [Route("/Units/{unitId}/DownloadRoster")]
    public ActionResult DownloadRoster(Guid unitId)
    {
      var download = service.GetRosterReport(unitId);
      return File(download.Stream, download.MimeType, download.Filename);
    }

    [Route("/api/units/{unitId}/roster")]
    public object ApiRoster(Guid unitId)
    {
      return service.GetRoster(unitId);
    }
  }
}
