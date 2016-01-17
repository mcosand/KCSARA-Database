﻿/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity;
  using System.Data.Entity.Validation;
  using System.Data.SqlClient;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.Profile;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsar.Membership;
  using Kcsara.Database.Geo;
  using Kcsara.Database.Web.Model;
  using log4net;
  using ApiModels = Kcsara.Database.Web.api.Models;

  public partial class AdminController : BaseController
  {
    public AdminController(IKcsarContext db) : base(db) { }

    [Authorize]
    public ActionResult Index()
    {
      return View();
    }

    [HttpGet]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Sql()
    {
      ViewBag.IsAccountAdmin = Permissions.IsInRole("site.accounts");
      return View();
    }

    public ContentResult UpdateDatabase(string updateKey)
    {
      StringBuilder result = new StringBuilder("Starting database update ...\n");

      var key = ConfigurationManager.AppSettings["DatabaseUpdateKey"];
      if (string.IsNullOrWhiteSpace(key))
      {
        return Content("DatabaseUpdateKey not in AppSettings");
      }

      if (!string.Equals(key, updateKey))
      {
        return Content("UpdateKey is not correct");
      }

      try
      {
        var setting = ConfigurationManager.ConnectionStrings["AuthStore"];
        if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
        {
          return Content("ConnectionString 'AuthStore' not set");
        }
        string authStore = setting.ConnectionString;

        setting = ConfigurationManager.ConnectionStrings["DataStore"];
        if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
        {
          return Content("ConnectionString 'DataStore' not set");
        }
        string dataStore = setting.ConnectionString;
        if (!dataStore.ToLowerInvariant().Contains("multipleactiveresultsets=true"))
        {
          return Content("DataStore connection string should include MultipleActiveResultSets=true");
        }

        result.AppendLine("Starting DataStore Migrations ...");
        result.AppendLine(dataStore);
        Kcsar.Database.Model.Migrations.Migrator.UpdateDatabase(dataStore);
        result.AppendLine("Migrations complete.");

        KcsarContext testContext = new KcsarContext();
        result.AppendLine("Default Context connection string: " + testContext.Database.Connection.ConnectionString);
        result.AppendFormat("Test Context returned {0} members\n", testContext.Members.Count());

        using (SqlConnection conn = api.AdminController.CreateAndOpenConnection(authStore, result))
        {
          bool tableExists;

          using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*)  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'aspnet_Applications'", conn))
          {
            tableExists = (int)cmd.ExecuteScalar() > 0;
          }
          if (!tableExists)
          {
            result.AppendLine("Creating authentication/authorization tables ...");
            result.AppendLine();
            api.AdminController.ExecuteSqlFile(conn, GetSqlContent("Sql", "AuthDatabase_Create.sql"), result);
            result.AppendLine();
            result.AppendLine("!!!! Default Admin user was created. Login = admin/password !!!!");
          }

          result.AppendLine();
          result.AppendLine("Updating authentication/authorization tables ...");
          api.AdminController.ExecuteSqlFile(conn, GetSqlContent("Sql", "AuthDatabase_Update.sql"), result);
        }

        result.AppendLine("Done");
      }
      catch (Exception ex)
      {
        result.AppendLine(ex.ToString());
      }
      return Content(result.ToString(), "text/plain");
    }

    private string GetSqlContent(params string[] relativePath)
    {
      return System.IO.File.ReadAllText(System.IO.Path.Combine(new[] { AppDomain.CurrentDomain.BaseDirectory }.Union(relativePath).ToArray()));
    }

    [Authorize(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Accounts()
    {
      ViewData["canedit"] = Permissions.IsInRole("site.accounts");
      return View(GetAccountsInGroup(null, false));
    }

    private AccountListRow[] GetAccountsInGroup(string group, bool sparse)
    {
      List<AccountListRow> list = new List<AccountListRow>();
      IEnumerable<MembershipUser> users = Membership.GetAllUsers().Cast<MembershipUser>();

      if (!string.IsNullOrWhiteSpace(group))
      {
        string[] inGroup = Roles.GetUsersInRole(group);
        users = users.Where(f => inGroup.Contains(f.UserName));
      }

      foreach (MembershipUser account in users)
      {
        list.Add(GetAccountView(account));
      }
      return list.ToArray();
    }

    private static AccountListRow GetAccountView(string username)
    {
      return GetAccountView(Membership.GetUser(username));
    }

    private static AccountListRow GetAccountView(MembershipUser account)
    {
      KcsarUserProfile profile = ProfileBase.Create(account.UserName) as KcsarUserProfile;

      AccountListRow row = new AccountListRow
      {
        Username = account.UserName,
        LastActive = account.LastLoginDate,
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        Email = account.Email,
        IsLocked = account.IsLockedOut,
      };
      return row;
    }

    [Authorize(Roles = "site.accounts")]
    public ActionResult DeleteUser(string id)
    {
      ViewData["PageTitle"] = "Delete User";

      if (Request.HttpMethod != "POST")
      {
        ModelState.SetModelValue("id", new ValueProviderResult(id, id, CultureInfo.CurrentUICulture));
      }
      else
      {
        MembershipUser user = Membership.GetUser(id);
        if (user == null)
        {
          ModelState.AddModelError("id", "Username not found");
        }

        if (ModelState.IsValid)
        {
          Membership.DeleteUser(id, true);
        }
        return RedirectToAction("Accounts");
      }

      return View();
    }

    [Authorize(Roles = "site.accounts")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult CreateUser()
    {
      ViewData["PageTitle"] = "Create New User";
      return View();
    }


    [HttpGet]
    [Authorize(Roles = "site.accounts")]
    public ActionResult EditUser(string id)
    {
      return View(GetAccountView(id));
    }

    [HttpPost]
    [Authorize(Roles = "site.accounts")]
    public ActionResult EditUser(string id, FormCollection fields)
    {
      AccountListRow row = GetAccountView(id);

      TryUpdateModel(row, new[] { "Email" });

      if (string.IsNullOrWhiteSpace(row.LinkKey))
      {
        TryUpdateModel(row, new[] { "LastName", "FirstName" });
      }

      if (row.LastName != fields["LastName"])
      {
        ModelState.SetModelValue("LastName", new ValueProviderResult(fields["LastName"], fields["LastName"], CultureInfo.CurrentUICulture));
        ModelState.AddModelError("LastName", "Can't be changed while Link is set");
      }

      if (row.FirstName != fields["FirstName"])
      {
        ModelState.SetModelValue("FirstName", new ValueProviderResult(fields["FirstName"], fields["FirstName"], CultureInfo.CurrentUICulture));
        ModelState.AddModelError("FirstName", "Can't be changed while Link is set");
      }

      if (ModelState.IsValid)
      {
        MembershipUser user = Membership.GetUser(id);
        KcsarUserProfile profile = ProfileBase.Create(user.UserName) as KcsarUserProfile;

        if (user.Email != row.Email) user.Email = row.Email;

        if (profile.LastName != row.LastName) profile.LastName = row.LastName;
        if (profile.FirstName != row.FirstName) profile.FirstName = row.FirstName;

        try
        {
          Membership.UpdateUser(user);
          profile.Save();
          ViewData["success"] = "Saved OK";
        }
        catch (Exception e)
        {
          ViewData["error"] = e.ToString();
        }
      }

      return View(row);
    }

    [Authorize(Roles = "site.accounts")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult CreateUser(string first, string last, string username, string email)
    {
      if (string.IsNullOrEmpty(first))
      {
        ModelState.AddModelError("first", "First name is required");
      }

      if (string.IsNullOrEmpty(username))
      {
        ModelState.AddModelError("username", "Username is required");
      }
      else if (Membership.GetUser(username) != null)
      {
        ModelState.AddModelError("username", "Username is already taken");
      }

      if (string.IsNullOrEmpty(email))
      {
        ModelState.AddModelError("email", "Email is required");
      }

      if (ModelState.IsValid)
      {
        string password = Membership.GeneratePassword(10, 3);
        Membership.CreateUser(username, password, email);


        KcsarUserProfile profile = ProfileBase.Create(username) as KcsarUserProfile;
        if (profile != null)
        {
          profile.FirstName = first;
          profile.LastName = last;
          profile.Save();
        }

        string newUserSubject = string.Format("New {0} User", ConfigurationManager.AppSettings["dbNameShort"] ?? "KCSARA");
        string newUserTemplate = (ConfigurationManager.AppSettings["emailNewUserBody"] ?? "Account has been created.\nUsername: {0}\nPassword: {1}").Replace("\\n", "\n");
        string newUserBody = string.Format(newUserTemplate, username, password);
        this.SendMail(email, newUserSubject, newUserBody);

        return RedirectToAction("Accounts");
      }
      return View();
    }

    [Authorize(Roles = "site.accounts")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ContentResult UnlockAccount(string id)
    {
      ((SqlMembershipProvider)Membership.Provider).UnlockUser(id);
      return new ContentResult { Content = id + " unlocked" };
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Groups()
    {
      var groups = Kcsar.Membership.RoleProvider.GetRoles().OrderBy(x => x.Name);
      List<GroupView> model = new List<GroupView>();
      string[] owners = groups.SelectMany(f => f.Owners).Distinct().ToArray();
      var ownerDetails = (owners.Length > 0) ? this.db.Members.Where(f => owners.Contains(f.Username)).ToDictionary(f => f.Username, f => f) : new Dictionary<string, Member>();

      foreach (var group in groups)
      {
        GroupView view = new GroupView { Name = group.Name, EmailAddress = group.EmailAddress, Destinations = group.Destinations.ToArray() };
        List<ApiModels.MemberSummary> ownersView = new List<ApiModels.MemberSummary>();
        foreach (string owner in group.Owners)
        {
          Member m = ownerDetails[owner];
          ownersView.Add(new ApiModels.MemberSummary
          {
            Name = m.FullName,
            Id = m.Id
          });
        }
        view.Owners = ownersView.ToArray();
        model.Add(view);
      }
      ViewData["IsAdmin"] = User.IsInRole("site.accounts");
      ViewData["UserId"] = Permissions.UserId;


      return View(model);
    }

    public DataActionResult GetAccounts()
    {
      if (!Permissions.IsAdmin) return GetLoginError();

      return Data(GetAccountsInGroup(null, false));
    }

    [Authorize(Roles = "cdb.users")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult GroupMembership(string id)
    {
      ExtendedRole group = Kcsar.Membership.RoleProvider.GetRole(id);
      INestedRoleProvider nested = Roles.Provider as INestedRoleProvider;

      ViewData["Title"] = "Membership for group " + id + " " + User.IsInRole("cdb.admins").ToString() + User.IsInRole("site.accounts").ToString() + User.IsInRole("admins").ToString();
      GroupMembershipView view = new GroupMembershipView
      {
        GroupName = group.Name,
        ShowGroups = false,
        CanEdit = CanUserEditRoleMembership(id)
      };

      var users = Membership.GetAllUsers();

      var currentUsers = Roles.GetUsersInRole(id);
      view.CurrentUsers = new SelectList(currentUsers);

      List<string> otherUsers = new List<string>();
      foreach (MembershipUser u in Membership.GetAllUsers())
      {
        if (!currentUsers.Contains(u.UserName))
        {
          otherUsers.Add(u.UserName);
        }
      }
      view.OtherUsers = new SelectList(otherUsers);


      if (nested != null)
      {
        view.ShowGroups = true;
        var currentGroups = nested.GetRolesInRole(id, false);
        view.CurrentGroups = new SelectList(currentGroups);
        view.OtherGroups = new SelectList(Roles.GetAllRoles().Where(g => !currentGroups.Contains(g)));
      }

      return View(view);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult GroupMembership(string id, FormCollection fields)
    {
      INestedRoleProvider nested = Roles.Provider as INestedRoleProvider;

      if (!CanUserEditRoleMembership(id))
      {
        return CreateLoginRedirect();
      }

      if (!string.IsNullOrEmpty(fields["AddGroup"]))
      {
        if (nested == null)
        {
          throw new InvalidOperationException("Configuration is using a RoleProvider that doesn't support nested groups");
        }

        foreach (string child in fields.GetValues("OtherGroups"))
        {
          nested.AddRoleToRole(child, id);
        }
      }
      else if (!string.IsNullOrEmpty(fields["RemoveGroup"]))
      {
        if (nested == null)
        {
          throw new InvalidOperationException("Configuration is using a RoleProvider that doesn't support nested groups");
        }

        foreach (string child in fields.GetValues("CurrentGroups"))
        {
          nested.RemoveRoleFromRole(child, id);
        }
      }
      else if (!string.IsNullOrEmpty(fields["AddUser"]))
      {
        Roles.AddUsersToRole(fields.GetValues("OtherUsers"), id);
      }
      else if (!string.IsNullOrEmpty(fields["RemoveUser"]))
      {
        Roles.RemoveUsersFromRole(fields.GetValues("CurrentUsers"), id);
      }

      return RedirectToAction("GroupMembership", new { id = id });
    }

    [Authorize]
    public ActionResult ReconcileActiveOnlyGroup(string group, string unit, string[] keep, bool? doit)
    {
      string msgs = string.Empty;
      doit = doit ?? false;

      var query = (from m in this.db.UnitMemberships where m.Status.IsActive && m.EndTime == null select m);
      if (!string.IsNullOrWhiteSpace(unit))
      {
        Guid unitId = UnitsController.ResolveUnit(this.db.Units, unit).Id;
        query = query.Where(f => f.Unit.Id == unitId);
      }
      List<string> desiredUsers = query.Select(f => f.Person.Username ?? ("*" + f.Person.FirstName + " " + f.Person.LastName)).ToList();
      List<string> usersToRemove = new List<string>();
      List<string> forceKeep = new List<string>();

      foreach (string user in Roles.GetUsersInRole(group))
      {
        if (desiredUsers.Contains(user))
        {
          desiredUsers.Remove(user);
        }
        else if (keep != null && keep.Contains(user))
        {
          forceKeep.Add(user);
        }
        else
        {
          usersToRemove.Add(user);
        }
      }
      if (desiredUsers.Count > 1)
      {
        msgs += string.Join("\n", desiredUsers.Where(f => f[0] != '*').Select(f => "add " + f).ToArray());
        msgs += string.Join("\n", desiredUsers.Where(f => f[0] == '*').Select(f => "No username for active member " + f.Substring(1)).ToArray());
        if (doit.Value && desiredUsers.Count(f => f[0] != '*') > 0) Roles.AddUsersToRole(desiredUsers.Where(f => f[0] != '*').ToArray(), group);
      }
      msgs += string.Join("\n", forceKeep.Select(f => "keeping " + f).ToArray());
      if (usersToRemove.Count > 1)
      {
        msgs += string.Join("\n", usersToRemove.Select(f => "remove " + f).ToArray());
        if (doit.Value) Roles.RemoveUsersFromRole(usersToRemove.ToArray(), group);
      }
      return new ContentResult { Content = msgs, ContentType = "text/plain" };
    }

    [Authorize(Roles = "site.accounts")]
    public ActionResult DeleteGroup(string id)
    {
      ViewData["PageTitle"] = "Delete User";

      if (Request.HttpMethod != "POST")
      {
        ModelState.SetModelValue("id", new ValueProviderResult(id, id, CultureInfo.CurrentUICulture));
      }
      else
      {
        if (!Roles.RoleExists(id))
        {
          ModelState.AddModelError("id", "Group not found");
        }

        if (ModelState.IsValid)
        {
          Roles.DeleteRole(id);
        }
        return RedirectToAction("Groups");
      }

      return View();
    }

    [Authorize(Roles = "site.accounts")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult CreateGroup()
    {
      ViewData["PageTitle"] = "Create New Group";
      return View();
    }

    [Authorize(Roles = "site.accounts")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult CreateGroup(string name)
    {
      ViewData["PageTitle"] = "Create New Group";
      if (string.IsNullOrEmpty(name))
      {
        ModelState.AddModelError("name", "Group name is required");
      }

      if (Roles.RoleExists(name))
      {
        ModelState.AddModelError("name", "Group already exists");
      }

      if (ModelState.IsValid)
      {
        Roles.CreateRole(name);

        return RedirectToAction("Groups");
      }
      return View();
    }

    [HttpGet]
    [Authorize(Roles = "site.accounts")]
    public ActionResult EditGroup(string id)
    {
      ExtendedRole role = Kcsar.Membership.RoleProvider.GetRole(id);
      ViewData["Destinations"] = string.Join(", ", role.Destinations.ToArray());
      ViewData["Owners"] = string.Join(", ", role.Owners.Select(f => f.ToString()).ToArray());
      return View(role);
    }

    [HttpPost]
    [Authorize(Roles = "site.accounts")]
    public ActionResult EditGroup(string id, FormCollection fields)
    {
      INestedRoleProvider nested = Roles.Provider as INestedRoleProvider;
      ExtendedRole role = nested.ExtendedGetRole(id);
      string origName = role.Name;

      TryUpdateModel(role, new[] { "EmailAddress", "Name" });

      ModelState.SetModelValue("Owners", new ValueProviderResult(fields["Owners"], fields["Owners"], CultureInfo.CurrentUICulture));
      role.Owners.Clear();
      role.Owners.AddRange((fields["Owners"] ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()));

      ModelState.SetModelValue("Destinations", new ValueProviderResult(fields["Destinations"], fields["Destinations"], CultureInfo.CurrentUICulture));
      role.Destinations.Clear();
      role.Destinations.AddRange((fields["Destinations"] ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()));

      try
      {
        nested.UpdateRole(role, origName);
        ViewData["success"] = "Saved OK";
      }
      catch (Exception e)
      {
        ViewData["error"] = e.ToString();
      }

      return View(role);
    }

    private bool CanUserEditRoleMembership(string id)
    {
      if (User.IsInRole("site.accounts"))
      {
        return true;
      }

      INestedRoleProvider nested = Roles.Provider as INestedRoleProvider;
      if (nested == null)
      {
        return false;
      }

      ExtendedRole role = nested.ExtendedGetRole(id);

      return role.Owners.Contains(Permissions.Username);
    }

    [Authorize(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult DisconnectedPhotos()
    {
      string storePath = Server.MapPath(MembersController.PhotosStoreRelativePath);
      var photoFiles = (from m in this.db.Members where m.PhotoFile != "" && m.PhotoFile != null select m.PhotoFile.ToLower()).ToList();

      List<string> existingFiles = System.IO.Directory.GetFiles(storePath).Select(f => f.Substring(storePath.Length)).ToList();
      for (int i = 0; i < existingFiles.Count; i++)
      {
        int dbIdx = photoFiles.IndexOf(existingFiles[i].ToLower());
        if (dbIdx >= 0 || existingFiles[i] == MembersController.StandInPhotoFile.ToLower())
        {
          if (dbIdx >= 0)
          {
            photoFiles.RemoveAt(dbIdx);
          }
          existingFiles.RemoveAt(i);
          i--;
        }
      }

      return View(existingFiles);
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult FixUnitMemberships()
    {
      Kcsar.Database.Model.UnitMembership lastUm = null;

      foreach (Kcsar.Database.Model.UnitMembership um in (from u in this.db.UnitMemberships.Include("Person").Include("Unit") select u).OrderBy(f => f.Person.Id).ThenBy(f => f.Unit.Id).ThenBy(f => f.Activated))
      {
        if (lastUm != null && um.Person.Id == lastUm.Person.Id && um.Unit.Id == lastUm.Unit.Id)
        {
          lastUm.EndTime = um.Activated;
        }
        lastUm = um;
      }
      this.db.SaveChanges();

      return new ContentResult { Content = "OK" };
    }

    public ActionResult FixAddresses(int? id)
    {
      id = id ?? 0;

      string data = "<table>";
      int pageSize = 40;
      foreach (var addr in (from a in this.db.PersonAddress.Include("Person") where a.Quality == (int)GeocodeQuality.Unknown select a).OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName).Skip(id.Value * pageSize).Take(pageSize))
      {
        string oldAddr = addr.Street + "<br/>" + addr.City + " " + addr.State + " " + addr.Zip;

        GeographyServices.RefineAddressWithGeography(addr);

        data += string.Format("<tr><td><b>{0}</b></td><td style=\"white-space:nowrap\">{1}</td><td>{2}</td></tr>",
                            addr.Person.ReverseName,
                            oldAddr,
                            string.Format("Quality: {0}<br/>{2}",
                                addr.Quality,
                                HttpUtility.HtmlEncode(string.Format("[{0}][{1}][{2}][{3}]", addr.Street, addr.City, addr.State, addr.Zip)))
                            );
      }
      try
      {
        this.db.SaveChanges();
      }
      catch (DbEntityValidationException ex)
      {
        LogManager.GetLogger("AdminController").ErrorFormat("Validation error: {0}",
          string.Join("\n", ex.EntityValidationErrors.SelectMany(f => f.ValidationErrors.Select(g => g.PropertyName + ": " + g.ErrorMessage))));
        data += "<tr><td colspan=23>DIDN'T SAVE ANY CHANGES BECAUSE OF VALIDATION EXCEPTIONS. CHECK LOGS.</td></tr>";
      }
      return new ContentResult { Content = data + "</table>", ContentType = "text/html" };
    }

    private static string UnitNameAsGroupName(string groupName)
    {
      return groupName.ToLower().Replace(" ", "");
    }

    /// <summary>
    /// Get all members of unit "id". Get all usernames in "group". Compare the usernames of active members
    /// with members already in the group. Report on differences.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="group"></param>
    /// <param name="commit"></param>
    /// <returns></returns>
    [HttpGet]
    public ActionResult FixupUnitMemberGroups(string id, string group, bool? commit)
    {
      if (!Permissions.IsUserOrLocal(Request))
      {
        Response.StatusCode = 403;
        return CreateLoginRedirect();
      }
      string result = "";

      SarUnit unit = UnitsController.ResolveUnit(this.db.Units, id);
      if (string.IsNullOrEmpty(group))
      {
        group = UnitNameAsGroupName(unit.DisplayName) + ".members";
      }

      var unitMembers = (from m in this.db.UnitMemberships
                         where m.Unit.Id == unit.Id && m.EndTime == null && m.Status.GetsAccount
                         select m.Person).OrderBy(f => f.LastName).ThenBy(f => f.FirstName).Distinct().ToArray();

      var usersInGroup = Roles.GetUsersInRole(group).ToList();

      foreach (var member in unitMembers)
      {
        if (usersInGroup.Contains(member.Username))
        {
          result += string.Format("{0} in group as {1}<br/>", member.FullName, member.Username);
          usersInGroup.Remove(member.Username);
        }
        else
        {
          result += string.Format("## {0} not in group. Should add {1}<br/>", member.FullName, member.Username);
        }
      }

      foreach (string username in usersInGroup)
      {
        result += string.Format("## suggest remove {0}<br/>", username);
      }

      return new ContentResult { ContentType = "text/html", Content = "Done.\n" + result };
    }

    [Authorize(Roles = "cdb.admins")]
    [HttpGet]
    public ActionResult UpdateAccountEmails()
    {
      string results = string.Empty;

      var map = this.db.Members.Include("ContactNumbers").ToDictionary(f => f.Id, f => f.ContactNumbers.Where(g => g.Type == "email").OrderBy(g => g.Priority).Select(g => g.Value).FirstOrDefault());

      foreach (MembershipUser user in Membership.GetAllUsers())
      {
        var view = GetAccountView(user);

        if (string.IsNullOrWhiteSpace(view.LinkKey))
        {
          results += user.UserName + " is not linked to database\n";
          continue;
        }

        Guid id = new Guid(view.LinkKey);
        if (!map.ContainsKey(id))
        {
          results += user.UserName + " has invalid database id: " + view.LinkKey + "\n";
          continue;
        }

        if (string.IsNullOrWhiteSpace(map[id]))
        {
          results += user.UserName + " has no email address in database\n";
          continue;
        }

        if (view.Email != map[id])
        {
          results += string.Format("{0} changing email from '{1}' to '{2}'\n", user.UserName, user.Email, map[id]);
          user.Email = map[id];
          Membership.UpdateUser(user);
        }
      }

      return new ContentResult { Content = results, ContentType = "text/plain" };
    }

    [Authorize(Roles = "cdb.admins")]
    public ContentResult ContactList(string id)
    {
      INestedRoleProvider nested = Roles.Provider as INestedRoleProvider;
      ExtendedRole role = nested.ExtendedGetRole(id);

      return Content(string.Join("; ", nested.GetUsersInRole(id, true).Select(f => Membership.GetUser(f).Email).Distinct().ToArray()), "text/plain");
    }

    [Authorize(Roles = "cdb.admins")]
    public DataActionResult GetInactiveMembersWithAccounts()
    {
      Member[] model;
      model = this.db.Members
          .Where(f => f.Username != null && !f.Memberships.Any(g => g.EndTime == null && g.Status.IsActive == true))
          .ToArray();

      return Data(model);
    }

    [Authorize]
    public ActionResult OnBehalfOf(string id)
    {
      if (!Permissions.IsAdmin) return GetLoginError();
      FormsAuthentication.SetAuthCookie(id, false);

      return RedirectToAction("Index", "Home");
    }

    #region TestMethods
    private static int counter = 0;
    [Authorize(Roles = "cdb.admins")]
    public DataActionResult TestExceptionHandling()
    {
      throw new InvalidCastException("Test exception " + (counter++).ToString());
    }

    [Authorize]
    public ContentResult TestMail()
    {
      string email = Membership.GetUser().Email;
      base.SendMail(email, "Test mail from KCSARA database.", "This test mail was sent from the database at " + DateTime.Now.ToString());
      return base.Content("Mail sent to " + email);
    }
    #endregion
  }
}