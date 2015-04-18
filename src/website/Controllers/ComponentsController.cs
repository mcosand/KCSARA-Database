/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System.Web.Mvc;

  public class ComponentsController : Controller
  {
    [HttpGet]
    public ActionResult Get(string id)
    {
      return PartialView(id);
    }

    [HttpGet]
    public ActionResult MemberEvents(string id)
    {
      ViewBag.EventType = id;
      return PartialView();
    }
  }
}