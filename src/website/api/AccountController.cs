/*
 * Copyright 2013-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using Kcsar.Database.Model;
  using SarMembership = Kcsar.Membership;
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
  using System.Web.Security;
  using Kcsara.Database.Services.Accounts;
  using System.Data.SqlClient;

  [ModelValidationFilter]
  public class AccountController : BaseApiController
  {
    public const string APPLICANT_ROLE = "cdb.applicants";
    readonly MembershipProvider membership;
    readonly AccountsService accountsService;
    readonly IFormsAuthentication formsAuth;

    public AccountController(IKcsarContext db, AccountsService accountsSvc, IFormsAuthentication formsAuth, ILog log, MembershipProvider membership)
      : base(db, log)
    {
      this.membership = membership;
      this.accountsService = accountsSvc;
    }

    [HttpPost]
    [AllowAnonymous]
    public string PartnerLogin(string authKey, string username)
    {
      if (!Regex.IsMatch(authKey, "[a-zA-Z0-9_-]+"))
      {
        throw new ArgumentException("Invalid auth key");
      }

      int siteId = 0;
      Guid? unitId = Guid.Empty;
      bool canDelegate = false;

      using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthStore"].ConnectionString))
      {
        conn.Open();
        string query = string.Format("SELECT siteId,unitId,canDelegate,defaultUser FROM partners WHERE authKey='{0}'", authKey);
        SqlCommand cmd = new SqlCommand(query, conn);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          if (siteId != 0)
          {
            throw new InvalidOperationException("Multiple matching keys");
          }
          siteId = reader.GetInt32(0);
          unitId = reader.IsDBNull(1) ? (Guid?)null : reader.GetGuid(1);
          canDelegate = reader.GetBoolean(2);
          // Assume we're going to be the default partner user.
          string user = reader.GetString(3);

          if (unitId != null && canDelegate && !string.IsNullOrWhiteSpace(username))
          {
            DateTime now = DateTime.Now;
            if (!this.db.Members.Any(f => f.Username == username && f.Memberships.Any(g => g.Activated < now && (g.EndTime == null || g.EndTime > now) && g.Unit.Id == unitId && g.Status.IsActive)))
            {
              throw new InvalidOperationException("Not able to delegate as " + username);
            }
            user = username;
          }

          formsAuth.SetAuthCookie(username, false);
        }
      }

      return "OK";
    }

    [HttpPost]
    [AllowAnonymous]
    public string Register(AccountRegistration data)
    {
      var emailCheck = CheckEmail(data.Email);
      if (emailCheck != RegistrationEmailStatus.Ready)
      {
        throw new InvalidOperationException("Email verification returned: " + emailCheck.ToString());
      }
      if (CheckUsername(data.Username) != "Available")
      {
        throw new InvalidOperationException("Username not available");
      }

      Guid memberId = Guid.Empty;
      var result = AddNewMember(data, () =>
      {
        var member = db.PersonContact.Where(f => f.Type == "email" && f.Value == data.Email).Select(f => f.Person).Single();
        memberId = member.Id;

        var now = DateTime.Now;

        // For all units where the member is active and they have accounts turned on...
        foreach (var unit in member.Memberships.Where(f => f.Activated < now && (f.EndTime == null || f.EndTime > now) && f.Status.GetsAccount).Select(f => f.Unit))
        {
          string roleName = string.Format("sec.{0}.members", unit.DisplayName.Replace(" ", "").ToLowerInvariant());

          // Give them rights as a member of the unit.
          if (System.Web.Security.Roles.RoleExists(roleName))
          {
            System.Web.Security.Roles.AddUserToRole(data.Username, roleName);
          }
        }

        return member;
      }, "register-account.html");

      if (result == "OK" && memberId != Guid.Empty)
      {
        var member = db.Members.Single(f => f.Id == memberId);
        member.Username = data.Username;
        db.SaveChanges();
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Email to check.</param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public RegistrationEmailStatus CheckEmail(string id)
    {
      return accountsService.CheckEmail((id ?? string.Empty).Trim());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Username to check.</param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public string CheckUsername(string id)
    {
      if (id.Length < 3)
        return "Too short";
      if (id.Length > 200)
        return "Too long";
      if (!Regex.IsMatch(id, @"^[a-zA-Z0-9\.\-_]+$"))
        return "Can only contain numbers, letters, and the characters '.', '-', and '_'";

      var existing = membership.GetUser(id, false);
      return (existing == null) ? "Available" : "Not Available";
    }

    [HttpPost]
    [AllowAnonymous]
    public string Signup(AccountSignup data)
    {
      if (string.IsNullOrWhiteSpace(data.Firstname))
        return "First name is required";
      if (string.IsNullOrWhiteSpace(data.Lastname))
        return "Last name is required";

      if (data.BirthDate > DateTime.Today.AddYears(-14))
        return "Applicants must be 14 years or older";
      if (data.BirthDate < DateTime.Today.AddYears(-120))
        return "Invalid birthdate";

      if (!(new[] { "m", "f", null }.Contains(data.Gender)))
        return "Invalid gender";

      if (data.Units.Length == 0)
        return "Must select at least one unit";

      return AddNewMember(data, () =>
      {
        Member newMember = new Member
        {
          FirstName = data.Firstname,
          MiddleName = data.Middlename,
          LastName = data.Lastname,
          BirthDate = data.BirthDate,
          Gender = (data.Gender == "m") ? Gender.Male
                  : (data.Gender == "f") ? Gender.Female
                  : Gender.Unknown,
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

        if (!System.Web.Security.Roles.RoleExists(APPLICANT_ROLE))
        {
          System.Web.Security.Roles.CreateRole(APPLICANT_ROLE);
        }
        System.Web.Security.Roles.AddUserToRole(data.Username, APPLICANT_ROLE);

        return newMember;
      }, "new-account-verification.html");
    }

    private string AddNewMember(AccountRegistration data, Func<Member> memberCallback, string noticeTemplate)
    {
      if (string.IsNullOrWhiteSpace(data.Email))
        return "Email is required";
      if (!Regex.IsMatch(data.Email, @"^\S+@\S+\.\S+$"))
        return "Unrecognized email address";


      if (string.IsNullOrWhiteSpace(data.Username))
        return "Username is required";
      if (data.Username.Length < 3)
        return "Username must be 3 or more characters";
      if (data.Username.Length > 200)
        return "Username must be less than 200 characters";
      if (!Regex.IsMatch(data.Username, @"^[a-zA-Z0-9\.\-_]+$"))
        return "Username can only contain numbers, letters, and the characters '.', '-', and '_'";
      if (membership.GetUser(data.Username, false) != null)
        return "Username is already taken";


      if (string.IsNullOrWhiteSpace(data.Password))
        return "Password is required";
      if (data.Password.Length < 6)
        return "Password must be at least 6 characters";
      if (data.Password.Length > 64)
        return "Password must be less than 64 characters";


      MembershipCreateStatus status;
      var user = membership.CreateUser(data.Username, data.Password, data.Email, null, null, false, null, out status);
      if (status != MembershipCreateStatus.Success)
        return "Could not create user";

      try
      {
        System.Web.Security.FormsAuthenticationTicket ticket = new System.Web.Security.FormsAuthenticationTicket(data.Username, false, 5);
        Thread.CurrentPrincipal = new System.Web.Security.RolePrincipal(new System.Web.Security.FormsIdentity(ticket));

        var member = memberCallback();

        SarMembership.KcsarUserProfile profile = ProfileBase.Create(data.Username) as SarMembership.KcsarUserProfile;
        if (profile != null)
        {
          profile.FirstName = member.FirstName;
          profile.LastName = member.LastName;
          profile.LinkKey = member.Id.ToString();
          profile.Save();
        }

        string mailSubject = string.Format("{0} account verification", ConfigurationManager.AppSettings["dbNameShort"] ?? "KCSARA");
        string mailTemplate = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Email", noticeTemplate));
        string mailBody = mailTemplate
            .Replace("%Username%", data.Username)
            .Replace("%VerifyLink%", new Uri(this.Request.RequestUri, Url.Route("Default", new { httproute = "", controller = "Account", action = "Verify", id = data.Username })).AbsoluteUri + "?key=" + user.ProviderUserKey.ToString())
            .Replace("%WebsiteContact%", ConfigurationManager.AppSettings["MailFrom"] ?? "database@kcsara.org");

        db.SaveChanges();
        EmailService.SendMail(data.Email, mailSubject, mailBody);
      }
      catch (Exception ex)
      {
        log.Error(ex.ToString());
        membership.DeleteUser(data.Username, true);
        return "An error occured while creating your user account";
      }

      return "OK";
    }

    [HttpPost]
    [AllowAnonymous]
    public bool Verify(AccountVerify data)
    {
      if (data == null || string.IsNullOrWhiteSpace(data.Username) || string.IsNullOrWhiteSpace(data.Key))
        return false;

      var user = membership.GetUser(data.Username, false);
      if (user != null && data.Key.Equals((user.ProviderUserKey ?? "").ToString(), StringComparison.OrdinalIgnoreCase))
      {
        user.IsApproved = true;
        membership.UpdateUser(user);

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
        membership.DeleteUser(name, true);
        var member = db.Members.Single(f => f.Username == name);
        member.Username = "-" + member.Username;
      }
      db.SaveChanges();

      return true;
    }
  }

  // The FormsAuthentication type is sealed and contains static members, so it is difficult to
  // unit test code that calls its members. The interface and helper class below demonstrate
  // how to create an abstract wrapper around such a type in order to make the AccountController
  // code unit testable.

  public interface IFormsAuthentication
  {
    void SetAuthCookie(string userName, bool createPersistentCookie);
    void SignOut();
  }

  public class FormsAuthenticationWrapper : IFormsAuthentication
  {
    public void SetAuthCookie(string userName, bool createPersistentCookie)
    {
      FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
    }
    public void SignOut()
    {
      FormsAuthentication.SignOut();
    }
  }
}
