namespace Sar.Auth.Controllers
{
  using System.Threading.Tasks;
  using System.Web;
  using System.Web.Mvc;
  using IdentityServer3.Core;
  using IdentityServer3.Core.Extensions;
  using Serilog;

  public class RegisterController : Controller
  {
    private readonly ILogger _log;

    public RegisterController(ILogger log)
    {
      _log = log;
    }

    // GET: Account
    public ActionResult Register()
    {
      return View();
    }

    [HttpGet]
    public async Task<ActionResult> RegisterLogin()
    {
      // this verifies that we have a partial signin from idsvr
      var ctx = Request.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      
      if (partial_login == null)
      {
        return View("Error");
      }

      ViewBag.Email = (partial_login.FindFirst(Constants.ClaimTypes.Email)
                   ?? partial_login.FindFirst("upn"))?.Value;
      ViewBag.Name = partial_login.FindFirst(Constants.ClaimTypes.Name)?.Value ?? Strings.UnknownUserName;

      return View();
    }
  }
}