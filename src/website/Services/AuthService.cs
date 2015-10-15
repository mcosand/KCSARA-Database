﻿/*
 * Copyright 2010-2015 Matthew Cosand
 */
 namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Principal;
  using System.Web;
  using System.Web.Profile;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsar.Membership;

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
        UserId = context.Members.Where(f => f.Username == user.Identity.Name).Select(f => f.Id).FirstOrDefault();
      }
    }

    public bool IsUserOrLocal(HttpRequestBase request)
    {
      var ips = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces().SelectMany(f =>
                      f.GetIPProperties().UnicastAddresses.Select(g =>
                              g.Address.ToString()));

      return IsUser || ips.Contains(request.UserHostAddress);
    }

    public bool IsSelf(Guid id)
    {
      return (UserId == id);
    }

    public bool IsSelf(string username)
    {
      return (user != null && user.Identity != null && user.Identity.IsAuthenticated && user.Identity.Name == username);
    }

    public bool IsAdmin
    {
      get { return IsInRole("cdb.admins"); }
    }

    public bool IsAuthenticated
    {
      get { return (user != null) && user.Identity.IsAuthenticated; }
    }

    public bool IsUser
    {
      get { return IsInRole("cdb.users"); }
    }

    public bool IsInRole(params string[] group)
    {
      if (user == null) return false;

      foreach (string g in group)
      {
        if (user.IsInRole(g)) return true;
      }
      return false;
    }

    public bool IsMembershipForPerson(Guid id)
    {
      return IsRoleForPerson("membership", id);
    }

    public bool IsRoleForPerson(string role, Guid personId)
    {
      Member member = (from m in context.Members.Include("Memberships.Status").Include("Memberships.Unit") where m.Id == personId select m).FirstOrDefault();
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
      string unitName = (from u in context.Units where u.Id == unitId select u.DisplayName).FirstOrDefault();
      return IsInRole(unitName.Replace(" ", "").ToLowerInvariant() + "." + role);
    }

    public bool IsMembershipForUnit(Guid id)
    {
      return IsRoleForUnit("membership", id);
    }

    public SarUnit[] GetUnitsIManage()
    {
      List<SarUnit> list = new List<SarUnit>();
      foreach (SarUnit u in (from unit in context.Units select unit))
      {
        if (IsInRole(u.DisplayName.Replace(" ", "").ToLowerInvariant() + ".membership"))
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

    public IEnumerable<string> GetGroupsForUser(string username)
    {
      return ((INestedRoleProvider)Roles.Provider).GetRolesForUser(username, false);
    }

    public IEnumerable<string> GetGroupsFoGroup(string group)
    {
      return ((INestedRoleProvider)Roles.Provider).GetRolesWithRole(group);
    }
  }
}
