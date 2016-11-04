using System;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Accounts;

namespace Sar.Database.Services
{
  public interface IUsersService
  {
    Task<Account> GetUser(Guid id);
    Task<Account> GetCurrentUser();
    Task<ListPermissionWrapper<Account>> List();
    Task<ItemPermissionWrapper<Account>> Get(Guid id);
    Task<Account> Save(Account account);
    Task Delete(Guid id);
    Task<Guid?> ResolveUsername(string id);

    Task SetPassword(Guid id, string newPassword);
    Task ResetPassword(Guid id, string resetUriTemplate);
    Task FinishResetPassword(string code, string newPassword);
  }
}
