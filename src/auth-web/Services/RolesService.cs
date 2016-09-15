using System;
using System.Collections.Generic;
using System.Linq;
using Sar.Auth.Data;
using Sar.Database.Services;
using Serilog;

namespace Sar.Database.Web.Auth.Services
{
  public class RolesService : IRolesService
  {
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ILogger _log;
    private readonly object cacheLock = new object();
    private Dictionary<string, List<string>> childrenCache = null;

    public RolesService(Func<IAuthDbContext> dbFactory, ILogger log)
    {
      _dbFactory = dbFactory;
      _log = log.ForContext<RolesService>();
    }

    public List<string> RolesForAccount(Guid accountId)
    {
      EnsureCache();

      using (var db = _dbFactory())
      {
        var roles = new List<string>();
        BuildRoleList(
          roles,
          db.Accounts.Where(f => f.Id == accountId).SelectMany(f => f.Roles).Select(f => f.Id));
        return roles;
      }
    }

    private void BuildRoleList(List<string> list, IEnumerable<string> newRoles)
    {
      foreach (var item in newRoles)
      {
        if (!list.Contains(item))
        {
          list.Add(item);
        }
        BuildRoleList(list, childrenCache[item]);
      }
    }

    private void EnsureCache()
    {
      if (childrenCache == null)
      {
        lock (cacheLock)
        {
          if (childrenCache == null)
          {
            using (var db = _dbFactory())
            {
              childrenCache = db.Roles.ToDictionary(
                f => f.Id,
                f => f.Children.Select(g => g.Id).ToList()
                );
            }
          }
        }
      }
    }
  }
}