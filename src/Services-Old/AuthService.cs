/*
 * Copyright 2010-2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Principal;
  using System.Web;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsar.Membership;

  public class AuthService : IAuthService
  {
    private IPrincipal user;
    private IKcsarContext context;
    public Guid UserId { get; private set; }
    public string Username { get; private set; }

    public AuthService(IPrincipal user, IKcsarContext context)
    {
      this.user = user;
      this.context = context;

      UserId = Guid.Empty;
      if (user != null)
      {
        UserId = context.Members.Where(f => f.Username == user.Identity.Name).Select(f => f.Id).FirstOrDefault();
      }
      Username = user.Identity.Name;
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
      MemberRow member = (from m in context.Members.Include("Memberships.Status").Include("Memberships.Unit") where m.Id == personId select m).FirstOrDefault();
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

    public SarUnitRow[] GetUnitsIManage()
    {
      return GetUnitsIManage_Internal().Values.ToArray();
    }

    private Dictionary<string,SarUnitRow> GetUnitsIManage_Internal()
    {
      var bag = new Dictionary<string, SarUnitRow>();
      foreach (SarUnitRow u in (from unit in context.Units select unit))
      {
        var group = u.DisplayName.Replace(" ", "").ToLowerInvariant() + ".membership";
        if (IsInRole(group))
        {
          bag.Add(group, u);
        }
      }
      return bag;
    }

    public void DeleteUser(string id)
    {
      Membership.DeleteUser(id);
    }

    public IEnumerable<string> GetGroupsIManage()
    {
      if (IsAdmin)
      {
        return Roles.GetAllRoles();
      }

      List<string> groups = GetUnitsIManage_Internal().Keys.ToList();
      groups.AddRange(((INestedRoleProvider)Roles.Provider).ExtendedGetAllRoles()
        .Where(f => f.Owners.Any(g => g == user.Identity.Name))
        .Select(f => f.Name)
        );

      return groups.Distinct();
    }

    public IEnumerable<string> GetGroupsForUser(string username)
    {
      return ((INestedRoleProvider)Roles.Provider).GetRolesForUser(username, false);
    }

    public IEnumerable<string> GetGroupsForGroup(string group)
    {
      return ((INestedRoleProvider)Roles.Provider).GetRolesWithRole(group);
    }

    public bool ValidateUser(string username, string password)
    {
      return Membership.ValidateUser(username, password);
    }
  }
}
