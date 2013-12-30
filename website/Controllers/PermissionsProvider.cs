

namespace Kcsara.Database.Web.Controllers
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

  public interface IPermissions
  {
    Guid UserId { get; }
    bool IsSelf(Guid id);
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
    bool IsUser { get; }
    bool InGroup(params string[] group);
    bool IsMembershipForPerson(Guid id);
    bool IsMembershipForUnit(Guid id);
    bool IsUserOrLocal(HttpRequestBase request);
    bool IsRoleForPerson(string role, Guid personId);
    bool IsRoleForUnit(string role, Guid unitId);
    
    void DeleteUser(string id);
  }

  public class PermissionsProvider : IPermissions
  {
    private IPrincipal user;
    private IKcsarContext context;
    public Guid UserId { get; private set; }

    public PermissionsProvider(IPrincipal user, IKcsarContext context)
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
      get { return this.InGroup("cdb.admins"); }
    }

    public bool IsAuthenticated
    {
      get { return (this.user != null) && this.user.Identity.IsAuthenticated; }
    }

    public bool IsUser
    {
      get { return this.InGroup("cdb.users"); }
    }

    public bool InGroup(params string[] group)
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
        if (this.InGroup(um.Unit.DisplayName.Replace(" ", "").ToLowerInvariant() + "." + role))
        {
          return true;
        }
      }
      return false;
    }

    public bool IsRoleForUnit(string role, Guid unitId)
    {
      string unitName = (from u in context.Units where u.Id == unitId select u.DisplayName).FirstOrDefault();
      return this.InGroup(unitName.Replace(" ", "").ToLowerInvariant() + "." + role);
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
        if (this.InGroup(u.DisplayName.Replace(" ", "").ToLowerInvariant() + ".membership"))
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
