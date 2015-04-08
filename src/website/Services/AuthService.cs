/*
 * Copyright 2010-2014 Matthew Cosand
 */


namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using System.Web.Security;
  using System.Security.Principal;
  using System.Web.Profile;
  using Kcsar.Membership;
  using Kcsar.Database.Model;
  using Kcsar.Database.Data;

  public class AuthService : IAuthService
  {
    private IPrincipal user;
    private IKcsarContext context;
    public Guid UserId { get; private set; }

    public AuthService(IPrincipal user, IKcsarContext context)
    {
      this.user = user;
      this.context = context;

      UserId = Guid.Empty;
      if (user != null)
      {
        UserId = UsernameToMemberKey(user.Identity.Name) ?? UserId;
      }
    }

    public static Guid? UsernameToMemberKey(string name)
    {
      KcsarUserProfile profile = ProfileBase.Create(name) as KcsarUserProfile;
      if (profile.UsesLink)
      {
        return new Guid(profile.LinkKey);
      }
      return null;
    }

    public bool IsUserOrLocal(HttpRequestBase request)
    {
      var ips = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().SelectMany(f =>
                      f.GetIPProperties().UnicastAddresses.Select(g =>
                              g.Address.ToString()));

      return this.IsUser || ips.Contains(request.UserHostAddress);
    }

    public bool IsSelf(Guid id)
    {
      return (UserId == id);
    }

    public bool IsAdmin
    {
      get { return this.IsInRole("cdb.admins"); }
    }

    public bool IsAuthenticated
    {
      get { return (this.user != null) && this.user.Identity.IsAuthenticated; }
    }

    public bool IsUser
    {
      get { return this.IsInRole("cdb.users"); }
    }

    public bool IsInRole(params string[] group)
    {
      if (this.user == null) return false;

      foreach (string g in group)
      {
        if (this.user.IsInRole(g)) return true;
      }
      return false;
    }

    public bool IsMembershipForPerson(Guid id)
    {
      return IsRoleForPerson("membership", id);
    }

    public bool IsRoleForPerson(string role, Guid personId)
    {
      MemberRow member = (from m in context.Members.Include("Memberships.Status").Include("Memberships.Unit") where m.Id == personId select m).FirstOrDefault();
      if (member == null)
      {
        return false;
      }

      foreach (UnitMembershipRow um in member.GetActiveUnits())
      {
        if (this.IsInRole(um.Unit.DisplayName.Replace(" ", "").ToLowerInvariant() + "." + role))
        {
          return true;
        }
      }
      return false;
    }

    public bool IsRoleForUnit(string role, Guid unitId)
    {
      string unitName = (from u in context.Units where u.Id == unitId select u.DisplayName).FirstOrDefault();
      return this.IsInRole(unitName.Replace(" ", "").ToLowerInvariant() + "." + role);
    }

    public bool IsMembershipForUnit(Guid id)
    {
      return IsRoleForUnit("membership", id);
    }

    public UnitRow[] GetUnitsIManage()
    {
      List<UnitRow> list = new List<UnitRow>();
      foreach (UnitRow u in (from unit in context.Units select unit))
      {
        if (this.IsInRole(u.DisplayName.Replace(" ", "").ToLowerInvariant() + ".membership"))
        {
          list.Add(u);
        }
      }
      return list.ToArray();
    }


    public void DeleteUser(string id)
    {
      Membership.DeleteUser(id);
    }
  }
}
