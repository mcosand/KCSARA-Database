/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer3.Core.Models;
using Sar.Auth.Data;

namespace Sar.Auth
{
  public static class Scopes
  {
    public static readonly string UnitsClaim = "units";
    public static readonly string MemberIdClaim = "memberId";
    public static readonly string RolesClaim = "role";

    public static IEnumerable<Scope> Get(Func<IAuthDbContext> dbFactory)
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
            StandardScopes.Profile,
            StandardScopes.EmailAlwaysInclude,
            new Scope { Name = "kcsara-profile", Type = ScopeType.Identity, IncludeAllClaimsForUser = true, Claims = new List<ScopeClaim>
            {
              new ScopeClaim(UnitsClaim, alwaysInclude: true),
              new ScopeClaim(MemberIdClaim, alwaysInclude: true),
              new ScopeClaim(RolesClaim, alwaysInclude: true)
            } }
        });

        return scopes;
      }
    }
  }
}