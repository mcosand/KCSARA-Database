/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Mvc;
  using Services;

  public class UnitsController : BaseController
  {
    private readonly IUnitsService service;
    
    public UnitsController(Lazy<IUnitsService> service, Lazy<IKcsarContext> db, ILog log) : base(db, log)
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
  }
}
