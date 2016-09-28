﻿using System.Configuration;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Kcsara.Database.Web.Controllers
{
  public class SinglePageController : Controller
  {
    [Authorize]
    public ActionResult AuthRequired(string page)
    {
      return View("SinglePage");
    }

    [Route("")]
    [Route("accounts/{*page}")]
    [Route("loggedIn")]
    [Route("units")]
    [Route("units/detail/{*page}")]
    [Route("units/roster/{*page}")]
    [Route("accounts/resetpassword")]
    public ActionResult Public(string page)
    {
      return View("SinglePage");
    }

    [HttpGet]
    [Route("loginSilent")]
    public ActionResult LoginSilent()
    {
      return View();
    }

    [HttpGet]
    [Authorize]
    [Route("login")]
    public RedirectResult Login(string returnUrl = null)
    {
      return Redirect(returnUrl ?? Url.Content("~/"));
    }

    [HttpGet]
    [Route("logout")]
    public ActionResult Logout()
    {
      var user = User as ClaimsPrincipal;
      var token = user?.FindFirst("id_token")?.Value;

      var url = ConfigurationManager.AppSettings["auth:authority"].TrimEnd('/') + "/connect/logout" + (token == null ? string.Empty : $"?id_token_hint={token}&post_logout_redirect_uri={Url.Content("~/")}");

      // Destroy the ASP.NET auth cookie.
      Request.GetOwinContext().Authentication.SignOut();
      // Redirect to IdP to destroy IdP cookies
      return Redirect(url);
    }
  }
}