namespace Kcsara.Database.Services.Accounts
{
  using System;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Kcsar.Database.Data;
  using log4net;

  public class AccountsService
  {
    readonly Func<IKcsarContext> dbFactory;
    readonly ILog log;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public AccountsService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
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
  }
}
