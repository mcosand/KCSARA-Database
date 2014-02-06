/*
 * Copyright 2010-2014 Matthew Cosand
 */


namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.SqlClient;
  using System.Linq;
  using System.Security.Principal;
  using System.Web;
  using System.Web.Profile;
  using System.Web.Security;
  using Kcsar.Database.Model;
  using Kcsar.Membership;
  using log4net;

  public class AuthService : IAuthService
  {
    private IPrincipal user;
    private IKcsarContext context;
    private readonly ILog log;
    public Guid UserId { get; private set; }

    public AuthService(IPrincipal user, IKcsarContext context, ILog log)
    {
      this.user = user;
      this.context = context;
      this.log = log;

      UserId = Guid.Empty;
      if (user != null && user.Identity.IsAuthenticated)
      {
        UserId = UsernameToMemberKey(user.Identity.Name) ?? UserId;
      }
    }

    private Guid? UsernameToMemberKey(string name)
    {
      KcsarUserProfile profile = ProfileBase.Create(name) as KcsarUserProfile;
      try
      {
        if (profile.UsesLink)
        {
          return new Guid(profile.LinkKey);
        }
        else
        {
          return this.context.Members.Where(f => f.Username == name).Select(f => f.Id).SingleOrDefault();
        }
      }
      catch (SqlException ex)
      {
        this.log.Warn("Exception setting up auth service", ex);
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
      Member member = (from m in context.Members.Include("Memberships.Status").Include("Memberships.Unit") where m.Id == personId select m).FirstOrDefault();
      if (member == null)
      {
        return false;
      }

      foreach (UnitMembership um in member.GetActiveUnits())
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

    public SarUnit[] GetUnitsIManage()
    {
      List<SarUnit> list = new List<SarUnit>();
      foreach (SarUnit u in (from unit in context.Units select unit))
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
