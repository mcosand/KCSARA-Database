/*
 * Copyright 2016 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using log4net;
  using Microsoft.AspNet.Mvc;
  using Services;
  using Model = Kcsar.Database.Model;

  public class AnimalsController : BaseController
  {
    readonly IAnimalsService service;

    public AnimalsController(Lazy<IAnimalsService> service, Lazy<Model.IKcsarContext> db, ILog log)
      : base(db, log)
    {
      this.service = service.Value;
    }


    [Route("/Animals")]
    public ActionResult Index()
    {
      ViewBag.ActiveMenu = "Animals";
      return View();
    }
  }
}