/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Linq;
  using System.Security.Principal;
  using System.Text.RegularExpressions;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using log4net;
  using ObjectModel.Accounts;

  public class AccountsService
  {
    readonly Func<IKcsarContext> dbFactory;
    readonly ILog log;
    readonly Func<IPrincipal> idGetter;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public AccountsService(Func<IPrincipal> idGetter, Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
      this.idGetter = idGetter;
    }

    public AccountInfo Get(string id)
    {
      id = id ?? idGetter().Identity.Name;

      var user = Membership.GetUser(id, false);
      if (user == null)
      {
        return null;
      }
      return new AccountInfo
      {
        Name = id,
        Approved = user.IsApproved,
        Locked = user.IsLockedOut,
        LastActive = user.LastActivityDate,
        LastPassword = user.LastPasswordChangedDate,
        LastLocked = user.LastLockoutDate < new DateTime(1900, 1, 1) ? (DateTimeOffset?)null : user.LastLockoutDate,
        Email = user.Email
      };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public RegistrationEmailStatus CheckEmail(string email)
    {
      RegistrationEmailStatus status = RegistrationEmailStatus.Invalid;

      using (var db = this.dbFactory())
      {
        if (!Regex.IsMatch(email, @"^\S+@\S+\.\S+$"))
        {
          // status = invalid
        }
        else
        {
          var usernames = db.Members.Where(f => f.ContactNumbers.Any(g => g.Type == "email" && g.Value == email)).Select(f => f.Username).ToArray();

          if (usernames.Length == 0)
          {
            status = RegistrationEmailStatus.NotFound;
          }
          else if (usernames.Length > 1)
          {
            status = RegistrationEmailStatus.Multiple;
          }
          else if (!string.IsNullOrEmpty(usernames[0]))
          {
            status = RegistrationEmailStatus.Registered;
          }
          else
          {
            status = RegistrationEmailStatus.Ready;
          }
        }
      }
      return status;
    }

    public MembershipUser Create(string username, string password, string email)
    {
      MembershipCreateStatus status;
      var user = Membership.CreateUser(username, password, email, null, null, false, null, out status);
      if (status != MembershipCreateStatus.Success)
      {
        throw new ApplicationException("Couldn't create user");
      }

      return user;
    }

    public void Delete(string username)
    {
      Membership.DeleteUser(username, true);
    }

    public bool ApproveUser(string username, string key)
    {
      var user = Membership.GetUser(username, false);

      if (user != null && key.Equals((user.ProviderUserKey ?? "").ToString(), StringComparison.OrdinalIgnoreCase))
      {
        user.IsApproved = true;
        Membership.UpdateUser(user);

        return true;
      }

      return false;
    }
  }
}
