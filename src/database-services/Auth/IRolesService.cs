using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sar.Database.Model.Accounts;

namespace Sar.Database.Services
{
  public interface IRolesService
  {
    List<string> ListAllRolesForAccount(Guid accountId);
    List<Role> ListRolesForAccount(Guid accountId);
    Task AddAccount(string role, Guid accountId);
  }
}
