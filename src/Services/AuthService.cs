/*
 * Copyright 2010-2016 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.NetworkInformation;
  using System.Security.Claims;
  using System.Security.Principal;
  using System.Web;
  using Kcsar.Database.Model;

  public class AuthService : IAuthService
  {
    private readonly IPrincipal _principal;
    private readonly IKcsarContext _db;

    public AuthService(IPrincipal user, IKcsarContext db)
    {
      _principal = user;
      _db = db;

      Guid memberId;
      var userClaim = GetClaims(f => f.Type == "memberId").SingleOrDefault();
      Guid.TryParse(userClaim?.Value ?? string.Empty, out memberId);
      UserId = memberId;
    }

    public bool IsAdmin
    {
      get
      {
        return IsInRole("cdb.admins");
      }
    }

    public bool IsAuthenticated
    {
      get
      {
        return _principal.Identity.IsAuthenticated;
      }
    }

    public bool IsUser
    {
      get
      {
        return IsInRole("cdb.users");
      }
    }

    public Guid UserId { get; private set; }

    public bool IsInRole(params string[] group)
    {
      return group.Any(f => _principal.IsInRole(f));
    }

    public bool IsMembershipForPerson(Guid id)
    {
      return IsRoleForPerson("membership", id);
    }

    public bool IsMembershipForUnit(Guid id)
    {
      return IsRoleForUnit("membership", id);
    }

    public bool IsRoleForPerson(string role, Guid personId)
    {
      Member member = (from m in _db.Members.Include("Memberships.Status").Include("Memberships.Unit") where m.Id == personId select m).FirstOrDefault();
      if (member == null)
      {
        return false;
      }
      if (IsAdmin)
      {
        return true;
      }

      foreach (UnitMembership um in member.GetActiveUnits())
      {
        if (IsInRole(um.Unit.DisplayName.Replace(" ", "").ToLowerInvariant() + "." + role))
        {
          return true;
        }
      }
      return false;
    }

    public bool IsRoleForUnit(string role, Guid unitId)
    {
      string unitName = (from u in _db.Units where u.Id == unitId select u.DisplayName).FirstOrDefault();
      return IsInRole(unitName.Replace(" ", "").ToLowerInvariant() + "." + role);
    }

    public bool IsSelf(Guid id)
    {
      return UserId == id;
    }

    public bool IsUserOrLocal(HttpRequestBase request)
    {
      var ips = NetworkInterface
                .GetAllNetworkInterfaces()
                .SelectMany(f => f.GetIPProperties()
                                  .UnicastAddresses
                                  .Select(g => g.Address.ToString()));

      return IsUser || ips.Contains(request.UserHostAddress);
    }

    private IEnumerable<Claim> GetClaims(Func<Claim, bool> predicate)
    {
      var claimsIdentity = _principal.Identity as ClaimsIdentity;
      if (claimsIdentity == null)
      {
        return new Claim[0];
      }

      var claims = claimsIdentity.Claims;
      if (predicate != null)
      {
        claims = claims.Where(predicate);
      }
      return claims;
    }
  }
}
