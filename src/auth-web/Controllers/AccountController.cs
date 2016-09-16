using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ViewModels;
using Sar.Auth.Services;
using Sar.Services;

namespace Sar.Auth.Controllers
{
  public class AccountController : Controller
  {
    private readonly SarUserService _userService;
    private readonly IHost _config;

    public AccountController(SarUserService service, IHost config)
    {
      _userService = service;
      _config = config;
    }

    #region Login

    /// <summary>
    /// Loads the HTML for the login page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <param name="message">
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Login(LoginViewModel model, SignInMessage message)
    {
      var openIdProviders = (_config.GetConfig("openId:providers") ?? string.Empty).Split(',');
      ViewBag.OpenIdIcons = openIdProviders.ToDictionary(f => f, f => "fa-" + _config.GetConfig("openId:" + f + ":fa-icon"));
      ViewBag.OpenIdColors = openIdProviders.ToDictionary(f => f, f => _config.GetConfig("openId:" + f + ":icon-color"));

      return View(model);
    }

    #endregion

    #region Logout

    /// <summary>
    /// Loads the HTML for the logout prompt page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Logout(LogoutViewModel model)
    {
      return View(model);
    }

    #endregion

    #region LoggedOut

    /// <summary>
    /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult LoggedOut(LoggedOutViewModel model)
    {
      return View(model);
    }

    #endregion

    #region Consent

    /// <summary>
    /// Loads the HTML for the user consent page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Consent(ConsentViewModel model)
    {
      return View(model);
    }

    #endregion

    #region Permissions

    // The custom MvcViewService wants to find this here in order to propagate the model.
    public ActionResult Error(ErrorViewModel model)
    {
      ViewBag.ErrorMessage = model.ErrorMessage;
      ViewBag.RequestId = model.RequestId;
      return View("AuthError", model);
    }

    /// <summary>
    /// Loads the HTML for the client permissions page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Permissions(ClientPermissionsViewModel model)
    {
      return View(model);
    }
    #endregion
  }
}