using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Sar.Auth.Data;
using Sar.Database.Data;
using Sar.Database.Model;
using Sar.Database.Model.Accounts;
using Sar.Database.Services;
using Serilog;

namespace Sar.Database.Web.Auth.Services
{
  public class ApiUsersService : IUsersService
  {
    private readonly IAuthorizationService _authz;
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly IHost _host;
    private readonly ILogger _log;
    private readonly ISendEmailService _emails;

    public ApiUsersService(Func<IAuthDbContext> dbFactory, ISendEmailService emails, IAuthorizationService authz, IHost host, ILogger log)
    {
      _log = log;
      _host = host;
      _authz = authz;
      _emails = emails;
      _dbFactory = dbFactory;
    }

    public Task Delete(Guid id)
    {
      throw new NotImplementedException();
    }

    public async Task<ItemPermissionWrapper<Account>> Get(Guid id)
    {
      return _authz.Wrap(await GetUser(id));
    }

    public Task<Account> GetCurrentUser()
    {
      var subClaim = _host.User.FindFirst("sub")?.Value;
      Guid sub;
      if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out sub)) return null;

      return GetUser(sub);
    }

    public async Task<Account> GetUser(Guid id)
    {
      using (var db = _dbFactory())
      {
        return await db.Accounts.Where(f => f.Id == id).Select(f => new Account
        {
          Id = f.Id,
          Username = f.Username,
          FirstName = f.FirstName,
          LastName = f.LastName,
          Email = f.Email,
          LastLogin = f.LastLogin,
          HasExternalLogins = f.ExternalLogins.Any(),
          LockDate = f.LockReason != null ? f.Locked : null,
          LockReason = f.LockReason,
          MemberId = f.MemberId
        }).FirstOrDefaultAsync();
      }
    }


    public async Task<ListPermissionWrapper<Account>> List()
    {
      using (var db = _dbFactory())
      {
        var list = await db.Accounts.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).Select(f => new Account
        {
          Id = f.Id,
          FirstName = f.FirstName,
          LastName = f.LastName,
          Email = f.Email,
          Username = f.Username,
          LastLogin = f.LastLogin,
          HasExternalLogins = f.ExternalLogins.Any(),
          LockDate = f.LockReason != null ? f.Locked : null,
          LockReason = f.LockReason,
          MemberId = f.MemberId
        }).ToListAsync();


        return new ListPermissionWrapper<Account>
        {
          C = _authz.CanCreate<Account>(),
          Items = list.Where(f => _authz.Authorize(f.Id, "Read:Account")).Select(f => _authz.Wrap(f))
        };
      }
    }

    public async Task<Guid?> ResolveUsername(string id)
    {
      using (var db = _dbFactory())
      {
        return await db.Accounts.Where(f => f.Username == id).Select(f => f.Id).SingleOrDefaultAsync();
      }
    }

    public async Task<Account> Save(Account account)
    {
      Guid dummy;
      if (Guid.TryParse(account.Username, out dummy)) throw new InvalidOperationException("Invalid username");

      using (var db = _dbFactory())
      {
        var match = await db.Accounts.FirstOrDefaultAsync(f =>
          f.Id != account.Id &&
          f.Username == account.Username &&
          f.Username != null);
        if (match != null) throw new ModelErrorException("username", $"Account with name {account.Username} already exists");

        var updater = await ObjectUpdater.CreateUpdater(
          db.Accounts,
          account.Id,
          null);

        if (account.Id != Guid.Empty
          && updater.Instance.Username != account.Username
          && updater.Instance.Username != null)
        {
          throw new UserErrorException("Can't change username", "Tried to change username '" + updater.Instance.Username + "' to '" + account.Username);
        }
        else
        {
          updater.Update(f => f.Username, account.Username);
        }

        updater.Update(f => f.Email, account.Email);
        updater.Update(f => f.FirstName, account.FirstName);
        updater.Update(f => f.LastName, account.LastName);
        if (account.LockReason != updater.Instance.LockReason)
        {
          if (_host.User.GetSubject() == account.Id)
          {
            throw new UserErrorException("Can't lock or unlock your own account",
              string.Format("Account {0} ({1}) tried to set own lock reason from {2} to {3}",
              account.Username,
              account.Id,
              updater.Instance.LockReason,
              account.LockReason));
          }
          bool unlock = string.IsNullOrWhiteSpace(account.LockReason);
          _log.Information("{0} account {username} ({accountId}) {lockReason}", unlock ? "Unlock" : "Lock", account.Username, account.Id, account.LockReason);
          updater.Update(f => f.Locked, unlock ? (DateTime?)null : DateTime.UtcNow);
          updater.Update(f => f.LockReason, account.LockReason);
        }
        await updater.Persist(db);

        return (await List()).Items.Single(f => f.Item.Id == updater.Instance.Id).Item;
      }
    }

    public async Task SetPassword(Guid id, string newPassword)
    {
      using (var db = _dbFactory())
      {
        var account = await db.Accounts.FirstOrDefaultAsync(f => f.Id == id);
        if (account == null) throw new NotFoundException("Not found", "Account", Convert.ToString(id));

        account.PasswordHash = SarUserService.GetSaltedPassword(newPassword);
        account.PasswordDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
      }
    }

    public async Task ResetPassword(Guid id, string resetUriTemplate)
    {
      using (var db = _dbFactory())
      {
        var account = await db.Accounts.FirstOrDefaultAsync(f => f.Id == id);
        if (account == null) return;

        if (string.IsNullOrWhiteSpace(account.Email))
        {
          _log.Error("Tried to reset password for account {accountId} but there's no email", id);
          return;
        }

        if (string.IsNullOrWhiteSpace(account.Username))
        {
          _log.Error("Tried to reset password for account {accountId} but it has no username", id);
          return;
        }

        _log.Information("Issuing password reset token for {username} to {email}", account.Username, account.Email);

        var token = Guid.NewGuid().ToString().Replace("-", "");
        account.ResetTokens.Add(new AccountResetTokenRow { Token = token });
        await db.SaveChangesAsync();

        var message = string.Format("A password change request was created for database account {0}. Please click the link below or copy/paste it into your " +
          "web browser.<br/><br/><a href=\"{1}\">{1}</a>", account.Username, string.Format(resetUriTemplate, token));
        await _emails.SendEmailAsync(account.Email, "Password Reset", message, true);
      }
    }

    public async Task FinishResetPassword(string code, string newPassword)
    {
      using (var db = _dbFactory())
      {
        var resetRow = await db.Accounts.SelectMany(f => f.ResetTokens, (a, t) => new { Account = a, Row = t }).FirstOrDefaultAsync();
        if (resetRow == null) throw new UserErrorException("Invalid or expired code", "Tried to reset password with code " + code);

        _log.Information("Resetting password for {username}", resetRow.Account.Username);

        resetRow.Account.PasswordHash = SarUserService.GetSaltedPassword(newPassword);
        resetRow.Account.PasswordDate = DateTime.UtcNow;
        resetRow.Account.ResetTokens.Clear();
        await db.SaveChangesAsync();
      }
    }
  }
}
