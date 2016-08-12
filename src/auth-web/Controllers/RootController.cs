/*
 * Copyright Matthew Cosand
 */
using System.Web.Mvc;

namespace Sar.Auth.Controllers
{
  public class RootController : Controller
  {
    [Route("jsconfig")]
    [HttpGet]
    public ContentResult JSConfig()
    {
      return Content(string.Format("window.appRoot = '{0}';", Url.Content("~/")), "text/javascript");
    }

    [Route("")]
    [HttpGet]
    public ActionResult Home()
    {
      return View("Index");
    }
  }
}