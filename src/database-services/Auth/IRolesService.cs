using System;
using System.Collections.Generic;

namespace Sar.Database.Services
{
  public interface IRolesService
  {
    List<string> ListAllRolesForAccount(Guid accountId);
  }
}
