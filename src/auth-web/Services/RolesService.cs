using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Sar.Auth.Data;
using Sar.Database.Model;
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
      _log.Information("ListAllRolesForAccount {accountId} {roles}", accountId, string.Join(",", roles));
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
          var list = part
                 .Select(f => cache[f])
                 .ToList();
          _log.Information("ListRolesForAccount {accountId} {roles}", accountId, string.Join(",", list));
          return list;
        }
      }
    }

    private void EnsureCache()
    {
      if (cacheTime < DateTime.UtcNow)
      {
        var newCache = new Dictionary<string, Role>();
        var newAccountToRoles = new Dictionary<Guid, string[]>();
        _log.Information("Rebuilding roles cache");
        using (var db = _dbFactory())
        {
          newCache = db.Roles.ToDictionary(
            f => f.Id,
            f => new Role { Id = f.Id, Name = f.Name, Description = f.Description }
          );

          foreach (var link in db.Roles.SelectMany(role => role.Children, (parent, child) => new { P = parent.Id, C = child.Id }))
          {
            newCache[link.P].Includes.Add(newCache[link.C]);
          }

          newAccountToRoles = db.Accounts.Include(f => f.Roles).ToDictionary(f => f.Id, f => f.Roles.Select(g => g.Id).ToArray());
        }


        lock (cacheLock)
        {
          if (cacheTime < DateTime.UtcNow)
          {
            _log.Information("Replacing roles cache");
            cache = newCache;
            accountToRolesCache = newAccountToRoles;
            cacheTime = DateTime.UtcNow.AddMinutes(5);
          }
        }
      }
    }

    public async Task AddAccount(string role, Guid accountId)
    {
      using (var db = _dbFactory())
      {
        var account = await db.Accounts.Include(f => f.Roles).FirstOrDefaultAsync(f => f.Id == accountId);
        account.Roles.Add(await db.Roles.SingleOrDefaultAsync(f => f.Id == role));
        await db.SaveChangesAsync();
        lock (cacheLock)
        {
          cacheTime = DateTime.MinValue;
        }
      }
    }
  }
}