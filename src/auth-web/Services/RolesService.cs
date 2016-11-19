using System;
using System.Collections.Generic;
using System.Linq;
using Sar.Auth.Data;
using Sar.Database.Model.Accounts;
using Sar.Database.Services;
using Serilog;

namespace Sar.Database.Web.Auth.Services
{
  public class RolesService : IRolesService
  {
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ILogger _log;
    private readonly object cacheLock = new object();
    private Dictionary<string, Role> cache = null;
    private Dictionary<Guid, string[]> accountToRolesCache = null;
    DateTime cacheTime = DateTime.MinValue;

    public RolesService(Func<IAuthDbContext> dbFactory, ILogger log)
    {
      _dbFactory = dbFactory;
      _log = log.ForContext<RolesService>();
    }

    public List<string> ListAllRolesForAccount(Guid accountId)
    {
      List<string> roles = null;
      string[] directRoleList = new string[0];

      EnsureCache();

      lock (cacheLock)
      {
        accountToRolesCache.TryGetValue(accountId, out directRoleList);

        roles = new List<string>();
        BuildRoleList(
          roles,
          directRoleList);
      }
      return roles;
    }

    private void BuildRoleList(List<string> list, IEnumerable<string> newRoles)
    {
      foreach (var item in newRoles)
      {
        if (!list.Contains(item))
        {
          list.Add(item);
        }
        BuildRoleList(list, cache[item].Includes.Select(f => f.Id));
      }
    }

    public List<Role> ListRolesForAccount(Guid accountId)
    {
      EnsureCache();

      using (var db = _dbFactory())
      {
        var part = db.Accounts.Where(f => f.Id == accountId)
                 .SelectMany(f => f.Roles).Select(f => f.Id).ToList();
        lock (cacheLock)
        {
          return part
                 .Select(f => cache[f])
                 .ToList();
        }
      }
    }

    private void EnsureCache()
    {
      if (cacheTime < DateTime.UtcNow)
      {
        lock (cacheLock)
        {
          if (cacheTime < DateTime.UtcNow)
          {
            using (var db = _dbFactory())
            {
              cache = db.Roles.ToDictionary(
                f => f.Id,
                f => new Role { Id = f.Id, Name = f.Name, Description = f.Description }
              );

              foreach (var link in db.Roles.SelectMany(role => role.Children, (parent, child) => new { P = parent.Id, C = child.Id }))
              {
                cache[link.P].Includes.Add(cache[link.C]);
              }

              accountToRolesCache = db.Accounts.ToDictionary(f => f.Id, f => f.Roles.Select(g => g.Id).ToArray());
            }
            cacheTime = DateTime.UtcNow.AddMinutes(5);
          }
        }
      }
    }
  }
}