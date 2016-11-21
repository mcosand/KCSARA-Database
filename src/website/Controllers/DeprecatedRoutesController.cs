using System.Web.Mvc;

namespace Kcsara.Database.Web.Controllers
{
  public class DeprecatedRoutesController : Controller
  {
    [Route("Account/Login")]
    [AuthorizeWithLog]
    public RedirectResult ToLogin(string returnUrl)
    {
      returnUrl = returnUrl ?? "/";
      return RedirectPermanent(returnUrl);
    }

    [Route("Account/Register")]
    public ContentResult Register()
    {
      return Content("Account registration is not working right now. If your email is recorded in the database you can link a Facebook or Google account");
    }
  }
}