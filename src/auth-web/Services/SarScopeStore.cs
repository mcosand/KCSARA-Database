using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Sar.Auth.Data;

namespace Sar.Database.Web.Auth.Services
{
  public class SarScopeStore : IScopeStore
  {
    public static readonly string UnitsClaim = "units";
    public static readonly string MemberIdClaim = "memberId";
    public static readonly string RolesClaim = "role";

    private readonly List<Scope> _scopes;

    public SarScopeStore(List<Scope> scopes)
    {
      _scopes = scopes;
    }

    public static SarScopeStore Create(Func<IAuthDbContext> dbFactory)
    {
      using (var db = dbFactory())
      {
        var scopes = db.Clients
          .Where(f => f.AddedScopes != null)
          .AsEnumerable()
          .SelectMany(f => f.AddedScopes.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
          .Distinct()
          .Select(f => new Scope { Name = f, Type = ScopeType.Resource })
          .ToList();

        scopes.AddRange(new[] {
            StandardScopes.OpenId,
            StandardScopes.ProfileAlwaysInclude,
            StandardScopes.EmailAlwaysInclude,
            new Scope { Name = "kcsara-profile", Type = ScopeType.Identity, Claims = new List<ScopeClaim>
            {
              new ScopeClaim(UnitsClaim, true),
              new ScopeClaim(MemberIdClaim, true),
              new ScopeClaim(RolesClaim, true)
            } }
        });

        var dbApiScope = scopes.FirstOrDefault(f => f.Name == "database-api") ?? new Scope { Name = "database-api", Type = ScopeType.Resource };
        dbApiScope.Claims = new List<ScopeClaim>
        {
          new ScopeClaim(UnitsClaim),
          new ScopeClaim(MemberIdClaim),
          new ScopeClaim(RolesClaim)
        };

        return new SarScopeStore(scopes);
      }
    }

    public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
    {
      return Task.FromResult(_scopes.Where(f => scopeNames.Contains(f.Name)));
    }

    public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
    {
      return Task.FromResult(_scopes.Where(f => f.ShowInDiscoveryDocument));
    }
  }
}
