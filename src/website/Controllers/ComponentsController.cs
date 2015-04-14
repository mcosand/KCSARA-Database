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
    public ActionResult SearchBox() { return PartialView(); }

    [HttpGet]
    public ActionResult LoginForm() { return PartialView(); }

    [HttpGet]
    public ActionResult MemberContacts() { return PartialView(); }

    [HttpGet]
    public ActionResult MemberAddresses() { return PartialView(); }

    [HttpGet]
    public ActionResult MemberEvents(string id)
    {
      ViewBag.EventType = id;
      return PartialView();
    }

    [HttpGet]
    public ActionResult TrainingRecords() { return PartialView(); }

    [HttpGet]
    public ActionResult CoreCompetencyStatus() { return PartialView(); }

    [HttpGet]
    public ActionResult TrainingRecordList() { return PartialView(); }
  }
}