using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using Kcsara.Database.Model.Members;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sar.Auth.Data;
using Sar.Services;
using Serilog;

namespace Sar.Auth.Services
{
  public class SarUserService : UserServiceBase
  {
    private readonly IMemberInfoService _memberService;
    private readonly IRolesService _roles;
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ISendEmailService _emailService;
    private readonly IHost _host;
    private readonly ILogger _log;

    private Dictionary<string, bool> unitStatusTypes = null;
    private object statusTypeLock = new object();

    public SarUserService(Func<IAuthDbContext> dbFactory, IMemberInfoService memberService, IRolesService roles, ISendEmailService email, IHost host, ILogger log)
    {
      _memberService = memberService;
      _roles = roles;
      _dbFactory = dbFactory;
      _emailService = email;
      _host = host;
      _log = log.ForContext<SarUserService>();
    }

    public async Task<object> GetUserAsync(string username)
    {
      using (var db = _dbFactory())
      {
        var user = await db.Accounts.SingleOrDefaultAsync(f => f.Username == username);

        if (user == null) return null;
        return new
        {
          Username = user.Username
        };
      }
    }

    public async Task<object> CreateUserAsync(ProvisionUserRequest info)
    {
      if (string.IsNullOrWhiteSpace(info.Username)) throw new ArgumentException("username is required");
      if (string.IsNullOrWhiteSpace(info.Email)) throw new ArgumentException("email is required");

      var existing = await GetUserAsync(info.Username);
      if (existing != null) {
        _log.Warning("Tried to create user " + info.Username + " but it already exists");
        throw new HttpException(400, "Username already exists");
      }

      using (var db = _dbFactory())
      {
        var salt = GetSalt();
        var hashed = string.IsNullOrWhiteSpace(info.Password)
                      ? null
                      : salt + HashPassword(info.Password, salt);

        var accountRow = new AccountRow
        {
          Id = Guid.NewGuid(),
          Username = info.Username,
          Email = info.Email,
          FirstName = info.FirstName,
          LastName = info.LastName,
          PasswordHash = hashed,
          PasswordDate = DateTime.UtcNow
        };

        db.Accounts.Add(accountRow);
        await db.SaveChangesAsync();
        _log.Information("Created account {account}", JsonConvert.SerializeObject(accountRow));

        await _emailService.SendEmailAsync(info.Email, "KCSARA Account Created", "Your King County Search and Rescue account has been created.\r\n\r\nUsername: " + info.Username);
        // Send mail to user
      }
      return await GetUserAsync(info.Username);
    }

    public async Task LinkMember(LinkMemberRequest body)
    {
      if (body == null) throw new ArgumentNullException(nameof(body));
      if (string.IsNullOrWhiteSpace(body.Username)) throw new ArgumentException("username is required");
      if (body.MemberId == Guid.Empty) throw new ArgumentException("memberid is required");

      using (var db = _dbFactory())
      {
        var accountRow = await db.Accounts.FirstOrDefaultAsync(f => f.Username == body.Username);
        if (accountRow == null)
        {
          _log.Information("Can't find account with username {username}", body.Username);
          throw new NotFoundException("not found", "Account", body.Username);
        }

        accountRow.MemberId = body.MemberId;
        await db.SaveChangesAsync();
        _log.Information("Linked account {username} to member {memberId}", body.Username, body.MemberId);
      }
    }

    public static string GetSaltedPassword(string password)
    {
      string salt = GetSalt();
      return salt + HashPassword(password, salt);
    }

    private static string GetSalt()
    {
      return GetSalt(PasswordSaltLength);
    }

    private static string GetSalt(int maximumSaltLength)
    {
      var salt = new byte[maximumSaltLength * 3 / 4];
      using (var random = new RNGCryptoServiceProvider())
      {
        random.GetNonZeroBytes(salt);
      }

      return Convert.ToBase64String(salt);
    }

    public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
    {
      using (var db = _dbFactory())
      {
        var login = await db.ExternalLogins.Include(f => f.Account)
                        .SingleOrDefaultAsync(f => f.Provider == context.ExternalIdentity.Provider && f.ProviderId == context.ExternalIdentity.ProviderId);
        var account = login?.Account;

        if (account == null)
        {
          context.AuthenticateResult = new AuthenticateResult("~/registerlogin", context.ExternalIdentity);
          return;
        }

        // This will be ignored if the user isn't registered.
        login.LastLogin = DateTime.Now;
        context.AuthenticateResult = await UpdateAccountOnLogin(db, account, context.ExternalIdentity);
      }
    }

    public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
    {
      context.AuthenticateResult = new AuthenticateResult(Strings.UserPasswordNotCorrect);

      using (var db = _dbFactory())
      {
        var account = await db.Accounts.SingleOrDefaultAsync(f => f.Username == context.UserName);

        context.AuthenticateResult = (account == null || string.IsNullOrWhiteSpace(account.PasswordHash) || !PasswordsMatch(context.Password, account.PasswordHash))
                                    ? new AuthenticateResult(Strings.UserPasswordNotCorrect)
                                    : await UpdateAccountOnLogin(db, account, null);
      }
    }

    private async Task<AuthenticateResult> UpdateAccountOnLogin(IAuthDbContext db, AccountRow account, ExternalIdentity externalIdentity)
    {
      if (account.Locked.HasValue)
      {
        _log.Warning(LogStrings.LockedAccountAttempt, account);
        return new AuthenticateResult(Strings.AccountLocked);
      }

      if (account.MemberId.HasValue)
      {
        var member = await _memberService.GetMember(account.MemberId.Value);
        if (member == null)
        {
          _log.Error(LogStrings.LinkedMemberNotFound, account.Username, account.Email);
          return new AuthenticateResult(Strings.AccountLocked);
        }

        if (member != null && (account.FirstName != member.FirstName || account.LastName != member.LastName || account.Email != member.Email))
        {
          account.FirstName = member.FirstName;
          account.LastName = member.LastName;
          if (!string.IsNullOrWhiteSpace(member.Email)) { account.Email = member.Email; }
        }
      }

      account.LastLogin = DateTime.Now;
      account.Logins.Add(new LoginLogRow { AccountId = account.Id, Time = account.LastLogin.Value, ProviderId = externalIdentity?.ProviderId });
      await db.SaveChangesAsync();

      if (string.IsNullOrWhiteSpace(account.FirstName) || string.IsNullOrWhiteSpace(account.LastName))
      {
        _log.Error(LogStrings.AccountHasNoName, account.Username);
      }

      return externalIdentity == null
        ? new AuthenticateResult(account.Id.ToString(), account.FirstName + " " + account.LastName)
        : new AuthenticateResult(account.Id.ToString(), account.FirstName + " " + account.LastName, null, externalIdentity.Provider);
    }

    public const int PasswordSaltLength = 24;

    public static string HashPassword(string password, string salt)
    {
      byte[] bytes = Encoding.Unicode.GetBytes(password);
      byte[] src = Convert.FromBase64String(salt);
      byte[] dst = new byte[src.Length + bytes.Length];
      Buffer.BlockCopy(src, 0, dst, 0, src.Length);
      Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
      HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
      byte[] inArray = algorithm.ComputeHash(dst);
      return Convert.ToBase64String(inArray);
    }

    public static bool PasswordsMatch(string password, string hashedPassword)
    {
      var salt = hashedPassword.Substring(0, PasswordSaltLength);
      var hashed = HashPassword(password, salt);
      return string.Equals(hashedPassword.Substring(PasswordSaltLength), hashed);
    }

    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
      List<Claim> claims = new List<Claim>();
      claims.Add(new Claim(Constants.ClaimTypes.ZoneInfo, "America/Los_Angeles"));

      if (unitStatusTypes == null)
      {
        var data = await _memberService.GetStatusToAccountMap();
        lock (statusTypeLock)
        {
          if (unitStatusTypes == null)
          {
            unitStatusTypes = data;
          }
        }
      }

      using (var db = _dbFactory())
      {
        var accountId = new Guid(context.Subject.GetSubjectId());
        var account = await db.Accounts.SingleOrDefaultAsync(f => f.Id == accountId);

        if (account.MemberId.HasValue)
        {
          var member = await _memberService.GetMember(account.MemberId.Value);
          if (member == null)
          {
            throw new InvalidOperationException(LogStrings.MemberNotFound);
          }
          if (!string.IsNullOrWhiteSpace(member.Email)) { account.Email = member.Email; }
          account.FirstName = member.FirstName;
          account.LastName = member.LastName;

          JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

          var units = member.Units.Select(f => JsonConvert.SerializeObject(new { f.Org.Id, f.Org.Name }, settings));
          foreach (var unit in units)
          {
            claims.Add(new Claim(Scopes.UnitsClaim, unit));
          }
          if (member.Units.Any(f => unitStatusTypes[(f.Org.Id.ToString() + f.Status).ToLowerInvariant()]))
          {
            claims.Add(new Claim(Scopes.RolesClaim, "cdb.users"));
          }

          claims.Add(new Claim(Scopes.MemberIdClaim, member.Id.ToString()));

          string profileTemplate = _host.GetConfig("memberProfileTemplate");
          if (!string.IsNullOrWhiteSpace(profileTemplate))
          {
            claims.Add(new Claim(Constants.ClaimTypes.Profile, string.Format(profileTemplate, member.Id)));
          }

          if (!string.IsNullOrWhiteSpace(member.PhotoUrl))
          {
            claims.Add(new Claim("picture", member.PhotoUrl));
          }
        }

        var roles = _roles.RolesForAccount(account.Id);
        foreach (var role in roles)
        {
          claims.Add(new Claim(Scopes.RolesClaim, role));
        }
      
        claims.Add(new Claim(Constants.ClaimTypes.Subject, account.Id.ToString()));
        claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
        claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean));
        claims.Add(new Claim(Constants.ClaimTypes.GivenName, account.FirstName));
        claims.Add(new Claim(Constants.ClaimTypes.FamilyName, account.LastName));
        claims.Add(new Claim(Constants.ClaimTypes.Name, account.FirstName + " " + account.LastName));
        

        context.IssuedClaims = context.AllClaimsRequested ? claims : claims.Where(f => context.RequestedClaimTypes.Contains(f.Type));
      }


    }

    public async Task<ProcessVerificationResult> VerifyExternalCode(ClaimsIdentity identity, string email, string code)
    {
      if (string.IsNullOrWhiteSpace(code))
      {
        throw new ArgumentNullException(nameof(code));
      }

      var nameIdClaim = identity.Claims.First(x => x.Type == Constants.ClaimTypes.ExternalProviderUserId);
      var provider = nameIdClaim.Issuer;
      var providerUserId = nameIdClaim.Value;

      using (var db = _dbFactory())
      {
        var verification = await db.Verifications.FirstOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId && f.Email == email);
        if (verification == null || verification.Code != code)
        {
          _log.Information(LogStrings.VerificationCodeNotCorrect, code, email);
          return ProcessVerificationResult.InvalidVerifyCode;
        }

        AccountRow account = null;
        var processResult = await ProcessVerification(email, provider, providerUserId,
          db,
          m =>
          {
            account = db.Accounts.Where(f => f.MemberId == m.Id).FirstOrDefault();
            if (account == null)
            {
              account = new AccountRow { Email = email, MemberId = m.Id, Created = DateTime.Now };
              db.Accounts.Add(account);
            }
            account.FirstName = m.First;
            account.LastName = m.Last;
          },
          a => { account = a; });
        if (processResult != ProcessVerificationResult.Success) return processResult;

        var login = new ExternalLoginRow { Account = account, Provider = provider, ProviderId = providerUserId, Created = DateTime.Now };
        db.ExternalLogins.Add(login);
        db.Verifications.Remove(verification);
        _log.Information(LogStrings.AssociatingExternalLogin, provider, providerUserId, account.FirstName + " " + account.LastName);
        await db.SaveChangesAsync();

        return ProcessVerificationResult.Success;
      }
    }

    private async Task<ProcessVerificationResult> ProcessVerification(string email, string provider, string providerUserId,
      IAuthDbContext db, Action<MemberInfo> memberAction, Action<AccountRow> accountAction)
    {
      var existingLogin = await db.ExternalLogins.FirstOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId);
      if (existingLogin != null)
      {
        _log.Warning(LogStrings.AlreadyRegistered,
          existingLogin.Provider,
          existingLogin.ProviderId,
          existingLogin.AccountId,
          existingLogin.Account.FirstName,
          existingLogin.Account.LastName);
        return ProcessVerificationResult.AlreadyRegistered;
      }

      var accounts = await db.Accounts.Where(f => f.Email == email).ToListAsync();
      if (accounts.Count == 0)
      {
        var members = await _memberService.FindMembersByEmail(email);
        if (members.Count == 0)
        {
          _log.Warning(LogStrings.EmailNotFound, email);
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (members.Count > 1)
        {
          _log.Warning(LogStrings.MultipleMembersForEmail, email, members.Select(f => new { Name = f.Name, Id = f.Id }));
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (memberAction != null)
        {
          memberAction(members[0]);
        }
      }
      else if (accounts.Count > 1)
      {
        _log.Warning(LogStrings.MultipleAccountsForEmail, email, accounts.Select(f => new { Name = f.FirstName + " " + f.LastName, Id = f.Id }));
        return ProcessVerificationResult.EmailNotAvailable;
      }
      else if (accountAction != null)
      {
        accountAction(accounts[0]);
      }

      return ProcessVerificationResult.Success;
    }

    public async Task<ProcessVerificationResult> SendExternalVerificationCode(ClaimsIdentity identity, string email)
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        throw new ArgumentNullException(nameof(email));
      }

      var nameIdClaim = identity.Claims.First(x => x.Type == Constants.ClaimTypes.ExternalProviderUserId);
      var provider = nameIdClaim.Issuer;
      var providerUserId = nameIdClaim.Value;

      using (var db = _dbFactory())
      {
        var processresult = await ProcessVerification(email, provider, providerUserId, db, null, null);
        if (processresult != ProcessVerificationResult.Success) return processresult;

        var verification = await db.Verifications.SingleOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId);
        if (verification == null)
        {
          verification = new VerificationRow { Provider = provider, ProviderId = providerUserId };
          db.Verifications.Add(verification);
        }

        verification.Created = DateTime.Now;
        verification.Code = Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", string.Empty);
        verification.Email = email;
        await db.SaveChangesAsync();

        _log.Information(LogStrings.SendingVerifyCode, email, provider, providerUserId);
        await _emailService.SendEmailAsync(email, Strings.VerifyMessageSubject, string.Format(Strings.VerifyMessageHtml, verification.Code));

        return ProcessVerificationResult.Success;
      }
    }
  }
}