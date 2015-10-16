/*
 * Copyright 2008-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Data.SqlClient;
  using System.Globalization;
  using System.Linq;
  using System.Security.Principal;
  using System.Threading;
  using System.Web.Mvc;
  using System.Web.Security;
  using System.Web.UI;
  using api.Models.Account;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web;
  using Kcsara.Database.Web.Model;
  using Config = System.Configuration;

  [OutputCache(Location = OutputCacheLocation.None)]
  public sealed class AccountController : BaseController
  {
    readonly Lazy<api.AccountController> accountsApi;

    public AccountController(Lazy<api.AccountController> accountsApi, IKcsarContext db, api.IFormsAuthentication formsAuth, System.Web.Security.MembershipProvider provider)
      : base(db)
    {
      Provider = provider;
      FormsAuth = formsAuth;
      this.accountsApi = accountsApi;
    }

    [Authorize]
    public ActionResult Detail(string id)
    {
      ViewBag.Account = accountsApi.Value.Get(id);
      if (ViewBag.Account == null)
      {
        return Content("You do not have permissions to view account details for " + id);
      }

      var perms = api.AccountController.GetPermissionsOnAccount(id, Permissions, db);
      if (!perms.HasFlag(api.AccountController.AccountPermissions.CanEdit))
      {
        return Content("You do not have permissions to view account details for " + id);
      }
      ViewBag.CanEdit = perms.HasFlag(api.AccountController.AccountPermissions.CanEdit);
      ViewBag.CanAdmin = perms.HasFlag(api.AccountController.AccountPermissions.CanAdmin);
      return View();
    }

    public ActionResult Signup()
    {
      ViewData["Title"] = "New Member Application";
      ViewData.Add("hideMenu", true);

      ViewBag.Units = this.db.Units.Where(f => f.NoApplicationsText != "never").OrderBy(f => f.LongName)
          .Select(f => new UnitApplicationInfoViewModel
          {
            Contact = f.Contacts.Where(g => g.Purpose == "applications" && g.Type == "email").Select(g => g.Value).FirstOrDefault(),
            Name = f.LongName,
            NoAppReason = f.NoApplicationsText,
            Id = f.Id
          });
      return View();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ActionResult Register()
    {
      ViewData["Title"] = "New Account Registration";
      ViewData.Add("hideMenu", true);

      return View();
    }

    public ActionResult Verify(string id, string key)
    {
      ViewData["Title"] = "Verify KCSARA Account";
      ViewData.Add("hideMenu", true);
      ViewBag.Username = id;
      ViewBag.Key = key;

      return View();
    }

    public ActionResult GetTicket(int p, string returnUrl)
    {
      if (User.Identity.IsAuthenticated)
      {
        return RedirectUserToPartnerTicketPage(p, returnUrl);
      }

      return RedirectToAction("Login", new { p = p, returnUrl = returnUrl });
    }

    [AcceptVerbs(HttpVerbs.Post)]
    public ContentResult CheckTicket(Guid t)
    {
      string username = "";

      using (SqlConnection conn = new SqlConnection(Config.ConfigurationManager.ConnectionStrings["AuthStore"].ConnectionString))
      {
        conn.Open();
        string query = string.Format("SELECT username FROM sso WHERE ticketId='{0}' AND expires > GETDATE()", t);
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          object value = cmd.ExecuteScalar();
          if (value != null)
          {
            username = (string)value;
          }
        }
      }

      List<string> values = new List<string>();
      values.Add(username);
      values.Add(string.Join(",", Roles.GetRolesForUser(username)));
      MembershipUser user = Membership.GetUser(username);

      values.Add((user == null) ? "" : user.Email);
      values.Add("[" + username + "]");

      return new ContentResult { Content = string.Join("\n", values.ToArray()) };
    }

    private ActionResult RedirectUserToPartnerTicketPage(int p, string returnUrl)
    {
      string redirect = "";
      Guid ticket = Guid.Empty;
      DateTime expires = DateTime.MinValue;

      using (SqlConnection conn = new SqlConnection(Config.ConfigurationManager.ConnectionStrings["AuthStore"].ConnectionString))
      {
        conn.Open();
        string query = string.Format("SELECT ticketId FROM sso WHERE siteId={0} AND username='{1}' AND expires > GETDATE()", p, User.Identity.Name);
        object dbTicket = null;
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          dbTicket = cmd.ExecuteScalar();
        }

        if (dbTicket == null)
        {
          ticket = Guid.NewGuid();
          query = string.Format("DELETE FROM sso WHERE username='{0}'", User.Identity.Name);
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.ExecuteNonQuery();
          }

          FormsAuthenticationTicket formTicket = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value);

          query = string.Format("INSERT INTO sso VALUES('{0}',{1},'{2}','{3}')",
                  ticket, p, User.Identity.Name, formTicket.Expiration.ToString("yyyy-MM-dd HH:mm:ss"));
          using (SqlCommand cmd = new SqlCommand(query, conn))
          {
            cmd.ExecuteNonQuery();
          }
        }
        else
        {
          ticket = (Guid)dbTicket;
        }

        query = string.Format("SELECT ticketPage FROM partners WHERE siteId={0}", p);
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
          redirect = (string)cmd.ExecuteScalar();
        }
      }

      redirect = redirect.Replace("%TICKET%", ticket.ToString());
      redirect = redirect.Replace("%URL%", returnUrl);
      return Redirect(redirect);
    }

    [Authorize]
    public ActionResult Index()
    {
      ViewData["PageTitle"] = "My Account";

      return View();
    }


    public api.IFormsAuthentication FormsAuth
    {
      get;
      private set;
    }

    public System.Web.Security.MembershipProvider Provider
    {
      get;
      private set;
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    //        [RequireHttps]
    public ActionResult ChangePassword()
    {

      ViewData["PageTitle"] = "Change Password";

      return View();
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    //        [RequireHttps]
    public ActionResult ChangePassword(string newPassword, string confirmPassword)
    {
      ViewData["PageTitle"] = "Change Password";

      if (newPassword == null)
      {
        ModelState.AddModelError("newPassword", "Must provide new password");
      }
      if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
      {
        ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
      }

      if (ModelState.IsValid)
      {
        try
        {
          string username = Thread.CurrentPrincipal.Identity.Name;
          ((Kcsar.Membership.ISetPassword)Membership.Provider).SetPassword(username, newPassword, false);

          return RedirectToAction("ChangePasswordSuccess");
        }
        catch (AggregateException ex)
        {
          ModelState.AddModelError("_FORM", string.Join("<br/>", ex.InnerExceptions.Select(f => string.Format("{0}: {1}", f.Data.Contains("name") ? f.Data["name"] : null, f.Message))));
        }
        catch (InvalidPasswordException)
        {
          ModelState.AddModelError("_FORM", "Password does not meet system requirements.");
        }
      }

      // If we got this far, something failed, redisplay form
      return View();
    }

    public ActionResult ChangePasswordSuccess()
    {

      ViewData["PageTitle"] = "Change Password";

      return View();
    }

    [AcceptVerbs(HttpVerbs.Get)]
    //        [RequireHttps]
    public ActionResult ResetPassword(string id)
    {
      ViewData["PageTitle"] = "Reset Password";

      if (Request.HttpMethod != "POST")
      {
        ModelState.SetModelValue("id", new ValueProviderResult(id, id, CultureInfo.CurrentUICulture));
      }
      else
      {
      }
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    //        [RequireHttps]
    public ActionResult ResetPassword(FormCollection fields)
    {
      MembershipUser user = null;

      ModelState.SetModelValue("id", new ValueProviderResult(fields["id"], fields["id"], CultureInfo.CurrentUICulture));
      if (string.IsNullOrEmpty(fields["id"]))
      {
        ModelState.AddModelError("id", "Username is required");
      }
      else
      {
        user = Membership.GetUser(fields["id"]);
        if (user == null)
        {
          ModelState.AddModelError("id", "User not found");
        }
      }

      if (ModelState.IsValid)
      {

        try
        {
          string password = ((SqlMembershipProvider)Membership.Provider).GeneratePassword();
          ((Kcsar.Membership.ISetPassword)Membership.Provider).SetPassword(fields["id"], password, true);

          return RedirectToAction("ResetPasswordDone", new { email = user.Email });
        }
        catch (AggregateException ex)
        {
          ModelState.AddModelError("_FORM", string.Join("<br/>", ex.InnerExceptions.Select(f => string.Format("{0}: {1}", f.Data.Contains("name") ? f.Data["name"] : null, f.Message))));
        }
      }

      return View();
    }

    public ActionResult ResetPasswordDone(string email)
    {
      ViewData["Email"] = email;
      return View();
    }

    //    [RequireHttps]
    public ActionResult Login()
    {

      ViewData["PageTitle"] = "Login";
      ViewData["HideMenu"] = true;
      return View();
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult ReAuth()
    {
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult Login(string username, string password, bool rememberMe, string returnUrl, int? id, int? p)
    {
      ViewData["PageTitle"] = "Login";

      // Basic parameter validation
      if (String.IsNullOrEmpty(username))
      {
        ModelState.AddModelError("username", "You must specify a username.");
      }
      if (String.IsNullOrEmpty(password))
      {
        ModelState.AddModelError("password", "You must specify a password.");
      }

      if (ViewData.ModelState.IsValid)
      {
        // Attempt to login
        bool loginSuccessful = Provider.ValidateUser(username, password);

        if (loginSuccessful)
        {
          FormsAuth.SetAuthCookie(username, rememberMe);
          if (p != null)
          {
            return RedirectToAction("GetTicket", new { p = p, returnUrl = returnUrl });
          }

          if (id.HasValue)
          {
            return Redirect(string.Format("{0}://{1}:{2}/{3}", this.Request.Url.Scheme, this.Request.Url.Host, id, returnUrl));
          }
          else if (!String.IsNullOrEmpty(returnUrl))
          {
            return Redirect(returnUrl);
          }
          else if (Roles.IsUserInRole(username, api.AccountController.APPLICANT_ROLE))
          {
            var member = db.Members.FirstOrDefault(f => f.Username == username);
            if (member != null)
            {
              return RedirectToAction("Detail", "Members", new { id = member.Id });
            }
          }
          return RedirectToAction("Index", "Home");
        }
        else
        {
          ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
        }
      }

      // If we got this far, something failed, redisplay form
      ViewData["rememberMe"] = rememberMe;
      return View();
    }

    public ActionResult Logout()
    {

      FormsAuth.SignOut();

      return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles="site.accounts")]
    public ActionResult InactiveMemberAccounts()
    {
      return View();
    }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (filterContext.HttpContext.User.Identity is WindowsIdentity)
      {
        throw new InvalidOperationException("Windows authentication is not supported.");
      }
    }


    private static string ErrorCodeToString(MembershipCreateStatus createStatus)
    {
      // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
      // a full list of status codes.
      switch (createStatus)
      {
        case MembershipCreateStatus.InvalidPassword:
          return "The password provided is invalid. Please enter a valid password value.";

        case MembershipCreateStatus.InvalidEmail:
          return "The e-mail address provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidAnswer:
          return "The password retrieval answer provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidQuestion:
          return "The password retrieval question provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidUserName:
          return "The user name provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.ProviderError:
          return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        case MembershipCreateStatus.UserRejected:
          return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        default:
          return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
      }
    }
  }
}
