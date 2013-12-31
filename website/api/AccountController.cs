
namespace Kcsara.Database.Web.api
{
  using Kcsar.Database.Model;
  using Kcsar.Membership;
  using Kcsara.Database.Web.api.Models;
  using Kcsara.Database.Web.Services;
  using System;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Web.Http;
  using System.Web.Profile;
  using Model = Kcsar.Database.Model;
  using log4net;

  [ModelValidationFilter]
  public class AccountController : BaseApiController
  {
    public const string APPLICANT_ROLE = "cdb.applicants";

    public AccountController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Username to check.</param>
    /// <returns></returns>
    [HttpPost]
    public string CheckUsername(string id)
    {
      if (id.Length < 3)
        return "Too short";
      if (id.Length > 200)
        return "Too long";
      if (!Regex.IsMatch(id, @"^[a-zA-Z0-9\.\-_]+$"))
        return "Can only contain numbers, letters, and the characters '.', '-', and '_'";

      var existing = System.Web.Security.Membership.GetUser(id);
      return (existing == null) ? "Available" : "Not Available";
    }

    [HttpPost]
    public string Signup(AccountSignup data)
    {
      if (string.IsNullOrWhiteSpace(data.Firstname))
        return "First name is required";
      if (string.IsNullOrWhiteSpace(data.Lastname))
        return "Last name is required";

      if (string.IsNullOrWhiteSpace(data.Email))
        return "Email is required";
      if (!Regex.IsMatch(data.Email, @"^\S+@\S+\.\S+$"))
        return "Unrecognized email address";

      if (data.BirthDate > DateTime.Today.AddYears(-14))
        return "Applicants must be 14 years or older";
      if (data.BirthDate < DateTime.Today.AddYears(-120))
        return "Invalid birthdate";

      if (!(new[] { "m", "f", null }.Contains(data.Gender)))
        return "Invalid gender";

      if (data.Units.Length == 0)
        return "Must select at least one unit";

      if (string.IsNullOrWhiteSpace(data.Username))
        return "Username is required";
      if (data.Username.Length < 3)
        return "Username must be 3 or more characters";
      if (data.Username.Length > 200)
        return "Username must be less than 200 characters";
      if (!Regex.IsMatch(data.Username, @"^[a-zA-Z0-9\.\-_]+$"))
        return "Username can only contain numbers, letters, and the characters '.', '-', and '_'";
      if (System.Web.Security.Membership.GetUser(data.Username) != null)
        return "Username is already taken";


      if (string.IsNullOrWhiteSpace(data.Password))
        return "Password is required";
      if (data.Password.Length < 6)
        return "Password must be at least 6 characters";
      if (data.Password.Length > 64)
        return "Password must be less than 64 characters";


      var user = System.Web.Security.Membership.CreateUser(data.Username, data.Password, data.Email);

      try
      {
        user.IsApproved = false;
        System.Web.Security.Membership.UpdateUser(user);

        System.Web.Security.FormsAuthenticationTicket ticket = new System.Web.Security.FormsAuthenticationTicket(data.Username, false, 5);
        Thread.CurrentPrincipal = new System.Web.Security.RolePrincipal(new System.Web.Security.FormsIdentity(ticket));

        Member newMember = new Member
        {
          FirstName = data.Firstname,
          MiddleName = data.Middlename,
          LastName = data.Lastname,
          BirthDate = data.BirthDate,
          InternalGender = data.Gender,
          Status = MemberStatus.Applicant,
          Username = data.Username
        };
        db.Members.Add(newMember);

        PersonContact email = new PersonContact
        {
          Person = newMember,
          Type = "email",
          Value = data.Email,
          Priority = 0
        };
        db.PersonContact.Add(email);

        foreach (Guid unitId in data.Units)
        {
          UnitsController.RegisterApplication(db, unitId, newMember);
        }

        KcsarUserProfile profile = ProfileBase.Create(data.Username) as KcsarUserProfile;
        if (profile != null)
        {
          profile.FirstName = data.Firstname;
          profile.LastName = data.Lastname;
          profile.LinkKey = newMember.Id.ToString();
          profile.Save();
        }

        if (!System.Web.Security.Roles.RoleExists(APPLICANT_ROLE))
        {
          System.Web.Security.Roles.CreateRole(APPLICANT_ROLE);
        }
        System.Web.Security.Roles.AddUserToRole(data.Username, APPLICANT_ROLE);

        string mailSubject = string.Format("{0} account verification", ConfigurationManager.AppSettings["dbNameShort"] ?? "KCSARA");
        string mailTemplate = File.ReadAllText(Path.Combine(Path.GetDirectoryName(new Uri(typeof(AccountController).Assembly.CodeBase).LocalPath), "EmailTemplates", "new-account-verification.html"));
        string mailBody = mailTemplate
            .Replace("%Username%", data.Username)
            .Replace("%VerifyLink%", new Uri(this.Request.RequestUri, Url.Route("Default", new { httproute = "", controller = "Account", action = "Verify", id = data.Username })).AbsoluteUri + "?key=" + user.ProviderUserKey.ToString())
            .Replace("%WebsiteContact%", "webpage@kingcountysar.org");

        db.SaveChanges();
        EmailService.SendMail(data.Email, mailSubject, mailBody);
      }
      catch (Exception ex)
      {
        log.Error(ex.ToString());
        System.Web.Security.Membership.DeleteUser(data.Username);
        return "An error occured while creating your user account";
      }

      return "OK";
    }

    [HttpPost]
    public bool Verify(AccountVerify data)
    {
      if (data == null || string.IsNullOrWhiteSpace(data.Username) || string.IsNullOrWhiteSpace(data.Key))
        return false;

      var user = System.Web.Security.Membership.GetUser(data.Username);
      if (user != null && data.Key.Equals((user.ProviderUserKey ?? "").ToString(), StringComparison.OrdinalIgnoreCase))
      {
        user.IsApproved = true;
        System.Web.Security.Membership.UpdateUser(user);

        return true;
      }

      return false;
    }

    [HttpPost]
    [Authorize(Roles = "site.accounts")]
    public object GetInactiveAccounts()
    {
      DateTime now = DateTime.Now;

      var members = this.db.Members.Where(f => 
        f.Id != Guid.Empty
        && f.Username != null && !f.Username.StartsWith("-")
        && (f.Status & MemberStatus.Applicant) != MemberStatus.Applicant
        && !f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null || g.EndTime > now)))
        .Select(f => new
        {
          Username = f.Username,
          FirstName = f.FirstName,
          LastName = f.LastName,
          Id = f.Id
        })
        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName)
        .ToArray();

      return members;
    }

    [HttpPost]
    [Authorize(Roles = "site.accounts")]
    public bool DisableAccounts(string[] id)
    {
      foreach (var name in id)
      {
        Permissions.DeleteUser(name);
        var member = db.Members.Single(f => f.Username == name);
        member.Username = "-" + member.Username;
      }
      db.SaveChanges();

      return true;
    }
  }
}