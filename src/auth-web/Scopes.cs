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
            StandardScopes.Email,
            new Scope { Name = "kcsara-profile", Type = ScopeType.Identity, Claims = new List<ScopeClaim>
            {
              new ScopeClaim(UnitsClaim),
              new ScopeClaim(MemberIdClaim),
              new ScopeClaim(RolesClaim)
            } }
        });

        return scopes;
      }
    }
  }
}