using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Accounts;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class AccountsController : ApiController
  {
    private readonly IAuthorizationService _authz;
    private readonly IUsersService _accounts;
    private readonly IRolesService _roles;
    private readonly ILog _log;

    public AccountsController(IUsersService accounts, IRolesService roles, IAuthorizationService authz, ILog log)
    {
      _accounts = accounts;
      _roles = roles;
      _authz = authz;
      _log = log;
    }

    [HttpGet]
    [Route("accounts")]
    public async Task<ListPermissionWrapper<Account>> List()
    {
      await _authz.EnsureAsync(null, "Read:Account");
      return await _accounts.List();
    }

    [HttpGet]
    [Route("accounts/{id}")]
    public async Task<ItemPermissionWrapper<Account>> Get(string id)
    {
      Guid accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Read:Account");
      return await _accounts.Get(accountId);
    }

    [HttpPost]
    [Route("accounts/{id}/password")]
    public async Task SetPassword(string id, [FromBody]string password)
    {
      var accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Update:AccountPassword");
      await _accounts.SetPassword(accountId, password);
    }

    [HttpPost]
    [Route("accounts/{id}/lock")]
    public async Task Lock(string id, [FromBody]string reason)
    {
      var accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Update:Account");
      var account = (await _accounts.Get(accountId)).Item;
      account.LockReason = reason;
      await _accounts.Save(account);
    }

    [HttpPost]
    [Route("accounts/{id}/unlock")]
    public async Task Unlock(string id)
    {
      var accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Update:Account");
      var account = (await _accounts.Get(accountId)).Item;
      account.LockReason = null;
      await _accounts.Save(account);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("accounts/{id}/resetpassword")]
    public async Task ResetPassword(string id)
    {
      var accountId = await ResolveAccountPassive(id);
      if (!accountId.HasValue) return;

      var resetTemplate = Url.Content("~/accounts/reset/").Replace("/api2", string.Empty) + "{0}";

      await _accounts.ResetPassword(accountId.Value, resetTemplate);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateModelState]
    [Route("accounts/resetpassword")]
    public async Task FinishResetPassword([FromBody] PasswordResetRequest request)
    {
      if (request == null) throw new UserErrorException("request data is required.");
      await _accounts.FinishResetPassword(request.Code, request.NewPassword);
    }

    [HttpGet]
    [Route("accounts/{id}/roles")]
    public async Task<List<Role>> GetRoles(string id)
    {
      Guid accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Read:Account");
      return _roles.ListRolesForAccount(accountId);
    }

    private async Task<Guid?> ResolveAccountPassive(string id, bool throwNotFound = false)
    {
      Guid accountId;
      if (!Guid.TryParse(id, out accountId))
      {
        Guid? temp;
        temp = await _accounts.ResolveUsername(id);
        if (!temp.HasValue)
        {
          if (throwNotFound)
          {
            throw new NotFoundException("Not found", "account", id);
          }
          else return null;
        }
        accountId = temp.Value;
      }

      return accountId;
    }

    private async Task<Guid> ResolveAccount(string id)
    {
      var result = await ResolveAccountPassive(id, true);
      return result.Value;
    }

    [HttpPost]
    [ValidateModelState]
    [Route("accounts")]
    public async Task<Account> CreateNew([FromBody]Account account)
    {
      await _authz.EnsureAsync(null, "Create:Account");

      if (account.Id != Guid.Empty)
      {
        throw new UserErrorException("New accounts shouldn't include an id");
      }

      account = await _accounts.Save(account);
      return account;
    }

    [HttpPut]
    [ValidateModelState]
    [Route("accounts/{id}")]
    public async Task<Account> Save(string id, [FromBody]Account account)
    {
      Guid accountId = await ResolveAccount(id);
      await _authz.EnsureAsync(accountId, "Update:Account");

      if (account.Id != accountId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      account = await _accounts.Save(account);
      return account;
    }

    [HttpDelete]
    [Route("accounts/{id}")]
    public async Task Delete(string id)
    {
      Guid accountId = await ResolveAccount(id);

      await _authz.EnsureAsync(accountId, "Delete:Account");
      await _accounts.Delete(accountId);
    }    
  }
}
