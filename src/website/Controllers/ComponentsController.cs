/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System.Web.Mvc;

  public class ComponentsController : Controller
  {
    [AllowAnonymous]
    [HttpGet]
    public ActionResult SearchBox()
    {
      return PartialView();
    }

    [HttpGet]
    public ActionResult LoginForm()
    {
      return PartialView();
    }
  }
}