/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity;
  using System.Data.SqlClient;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web.Http;
  using System.Web.Profile;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.api.Models;
  using log4net;
  using ObjectModel.Accounts;
  using SarMembership = Kcsar.Membership;

  [ModelValidationFilter]
  [CamelCaseControllerConfig]
  public class AccountController : BaseApiController
  {
    public const string APPLICANT_ROLE = "cdb.applicants";
    readonly AccountsService accountsService;
    readonly IFormsAuthentication formsAuth;

    public AccountController(IKcsarContext db, AccountsService accountsSvc, IFormsAuthentication formsAuth, ILog log)
      : base(db, log)
    {
      this.accountsService = accountsSvc;
      this.formsAuth = formsAuth;
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public AccountInfo Get(string id)
    {
      return accountsService.Get(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public AccountInfo Put(AccountInfo id)
    {
      var user = Membership.GetUser(id.Name);
      if (user == null)
      {
        throw new HttpResponseException(HttpStatusCode.BadRequest);
      }

      var perms = GetPermissionsOnAccount(id.Name, Permissions, db);
      if (!perms.HasFlag(AccountPermissions.CanEdit))
      {
        throw new HttpResponseException(HttpStatusCode.BadRequest);
      }

      if (!string.Equals(user.Email, id.Email, StringComparison.OrdinalIgnoreCase))
      {
        user.Email = id.Email;
      }

      if (perms.HasFlag(AccountPermissions.CanAdmin))
      {
        if (user.IsLockedOut && !id.Locked)
        {
          user.UnlockUser();
        }
        if (user.IsApproved != id.Approved)
        {
          user.IsApproved = id.Approved;
        }

        Member memberToLink = null;
        if (id.MemberId.HasValue)
        {
          memberToLink = db.Members.FirstOrDefault(f => f.Id == id.MemberId.Value);
          if (memberToLink == null)
          {
            // member not found
            throw new HttpResponseException(HttpStatusCode.BadRequest);
          }

          if (!string.IsNullOrWhiteSpace(memberToLink.Username) && !string.Equals(memberToLink.Username, id.Name, StringComparison.OrdinalIgnoreCase))
          {
            throw new InvalidOperationException("Can't link username " + id.Name + " to member " + memberToLink.Id.ToString() + ". Already linked to " + memberToLink.Username);
          }
          memberToLink.Username = id.Name;
          db.SaveChanges();
        }
      }

      Membership.UpdateUser(user);

      if (!string.IsNullOrWhiteSpace(id.Password))
      {
        ((Kcsar.Membership.ISetPassword)Membership.Provider).SetPassword(id.Name, id.Password, false);
      }

      return Get(id.Name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public AccountInfo ResetPassword(string id)
    {
      var user = Membership.GetUser(id);

      string password = ((SqlMembershipProvider)Membership.Provider).GeneratePassword();
      ((Kcsar.Membership.ISetPassword)Membership.Provider).SetPassword(id, password, true);

      return Permissions.IsAuthenticated
        ? Get(id)
        : new AccountInfo
          {
            Email = user.Email,
            Name = id
          };
    }

    [Flags]
    public enum AccountPermissions
    {
      None = 0,
      View = 1,
      CanEdit = 3,
      CanAdmin = 7
    }

    public static AccountPermissions GetPermissionsOnAccount(string username, IAuthService authService, IKcsarContext db)
    {
      var member = db.Members.SingleOrDefault(f => f.Username == username);
      bool canAdmin = authService.IsAdmin;
      bool canEdit = canAdmin;

      if (member != null)
      {
        canAdmin = authService.IsMembershipForPerson(member.Id);
        canEdit = canAdmin || authService.IsSelf(member.Id);

      }

      if (!canEdit)
      {
        return AccountPermissions.None;
      }

      return (canAdmin ? AccountPermissions.CanAdmin : AccountPermissions.None) | (canEdit ? AccountPermissions.CanEdit : AccountPermissions.None);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public IEnumerable<string> RolesIManage()
    {
      return Permissions.GetGroupsIManage();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public IEnumerable<string> RolesForUser(string id)
    {
      return Permissions.GetGroupsForUser(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public IEnumerable<string> RolesForRole(string id)
    {
      return Permissions.GetGroupsForGroup(id);
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

      var existing = accountsService.Get(id);
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
      if (accountsService.Get(data.Username) != null)
        return "Username is already taken";


      if (string.IsNullOrWhiteSpace(data.Password))
        return "Password is required";
      if (data.Password.Length < 6)
        return "Password must be at least 6 characters";
      if (data.Password.Length > 64)
        return "Password must be less than 64 characters";


      var user = accountsService.Create(data.Username, data.Password, data.Email);

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
        accountsService.Delete(data.Username);
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

      return accountsService.ApproveUser(data.Username, data.Key);
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
        accountsService.Delete(name);
        var member = db.Members.Single(f => f.Username == name);
        member.Username = "-" + member.Username;
      }
      db.SaveChanges();

      return true;
    }

    [HttpGet]
    [Authorize]
    public async Task<string> LookupExternalLogin(string provider, string login, string memberOf = null)
    {
      var query = this.db.Members.Where(f => f.ExternalLogins.Any(g => g.Provider == provider && g.Login == login));
      if (!string.IsNullOrWhiteSpace(memberOf))
      {
        DateTime now = DateTime.Now;
        query = query.Where(f => f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null || g.EndTime > now) && g.Unit.DisplayName == memberOf));
      }

      return await query.Select(f => f.Username).SingleOrDefaultAsync();
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
