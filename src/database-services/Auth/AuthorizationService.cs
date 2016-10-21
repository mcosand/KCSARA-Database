using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Model.Units;
using Data = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IAuthorizationService
  {
    Task<bool> AuthorizeAsync(object resource, string policyName, ClaimsPrincipal user = null);
    bool Authorize(object resource, string policyName, ClaimsPrincipal user = null);
    Task<bool> EnsureAsync(object resource, string policyName, bool throwIfDenied = true, ClaimsPrincipal user = null);
    bool CanCreate<T>() where T : class;
    ItemPermissionWrapper<T> Wrap<T>(T item) where T : class, IId;
  }

  public class AuthorizationService : IAuthorizationService
  {
    private readonly IRolesService _rolesService;
    private readonly IHost _host;
    private readonly Func<Data.IKcsarContext> _dbFactory;

    public AuthorizationService(Func<Data.IKcsarContext> dbFactory, IHost host, IRolesService rolesService)
    {
      _dbFactory = dbFactory;
      _host = host;
      _rolesService = rolesService;
    }

    public Task<bool> EnsureAsync(object resource, string policyName, bool throwIfDenied = true, ClaimsPrincipal user = null)
    {
      user = user ?? _host.User;
      var result = AuthorizeImpl(user, resource, policyName);
      if (result == false && throwIfDenied) throw new AuthorizationException();
      return Task.FromResult(result);
    }

    public Task<bool> AuthorizeAsync(object resource, string policyName, ClaimsPrincipal user = null)
    {
      user = user ?? _host.User;
      return Task.FromResult(AuthorizeImpl(user, resource, policyName));
    }

    public bool Authorize(object resource, string policyName, ClaimsPrincipal user = null)
    {
      user = user ?? _host.User;
      return AuthorizeImpl(user, resource, policyName);
    }

    private bool AuthorizeImpl(ClaimsPrincipal user, object resource, string policyName)
    {
      if (policyName == null) throw new ArgumentNullException(nameof(policyName));

      var memberIdString = user.FindFirst("memberId")?.Value;
      var memberId = string.IsNullOrWhiteSpace(memberIdString) ? (Guid?)null : new Guid(memberIdString);

      // Members can read their own records.
      if ((Guid?)resource == memberId && policyName.StartsWith("Read:") && (policyName.EndsWith("@Member") || policyName.EndsWith(":Member")))
      {
        return true;
      }

      Match m = Regex.Match(policyName, "^([a-zA-Z]+)(\\:([a-zA-Z]+)(@([a-zA-Z]+))?)?$");
      if (!m.Success) throw new InvalidOperationException("Unknown policy " + policyName);

      var roles = _rolesService.RolesForAccount(new Guid(user.FindFirst("sub").Value));
      var scopes = user.FindAll("scope").Select(f => f.Value).ToList();

      if (roles.Any(f => Equals(f, "cdb.admins")) || scopes.Any(f => f.StartsWith("db-w-")))
      {
        return true;
      }

      if (m.Groups[5].Value == "UnitId" || (m.Groups[3].Value == "Unit" && m.Groups[1].Value != "Create"))
      {
        using (var db = _dbFactory())
        {
          Guid unitId = (Guid)resource;
          var unitName = db.Units.SingleOrDefault(f => f.Id == unitId)?.DisplayName;
          if (roles.Any(f => string.Equals(f, $"cdb.{NormalizeGroupName(unitName)}.admins", StringComparison.OrdinalIgnoreCase))) return true;
        }
      }

      if (m.Groups[1].Value == "Read" && (
        roles.Any(f => f == "cdb.users")
        || scopes.Any(f => f.StartsWith("db-r-"))
        )) return true;

      return false;
    }

    private static string NormalizeGroupName(string name)
    {
      return name?.Replace(" ", string.Empty);
    }

    public bool CanCreate<T>() where T : class
    {
      return Authorize(null, "Create:" + typeof(T).Name);
    }

    public ItemPermissionWrapper<T> Wrap<T>(T item) where T : class, IId
    {
      var typeName = typeof(T).Name;
      var canUpdate = Authorize(item.Id, $"Update:{typeName}");
      var canDelete = Authorize(item.Id, $"Delete:{typeName}");

      if (typeof(T) == typeof(UnitStatusType))
      {
        var status = item as UnitStatusType;
        canUpdate |= Authorize(status.Unit.Id, "Update:UnitStatusType@UnitId");
        canDelete |= Authorize(status.Unit.Id, "Delete:UnitStatusType@UnitId");
        return PermissionWrapper.Create(item, canUpdate, canDelete);
      }
      else if (typeof(T) == typeof(TrainingRecord))
      {
        var record = item as TrainingRecord;
        canUpdate |= Authorize(record.Member.Id, "Update:TrainingRecord@MemberId")
                     || Authorize(record.Course.Id, "Update:TrainingRecord@CourseId")
                     || (record.Source == "roster" && Authorize(record.ReferenceId, "Update:TrainingRecord@ReferenceId"));
        canDelete |= Authorize(record.Member.Id, "Delete:TrainingRecord@MemberId")
                     || Authorize(record.Course.Id, "Delete:TrainingRecord@CourseId")
                     || (record.Source == "roster" && Authorize(record.ReferenceId, "Delete:TrainingRecord@ReferenceId"));
        return PermissionWrapper.Create(item, canUpdate, canDelete);
      }
      else if (typeof(T) == typeof(UnitMembership))
      {
        var m = item as UnitMembership;
        canUpdate |= Authorize(m.Member.Id, "Update:UnitMembership@MemberId")
                     || Authorize(m.Unit.Id, "Update:UnitMembership@UnitId");
        canDelete |= Authorize(m.Member.Id, "Delete:UnitMembership@MemberId")
                     || Authorize(m.Unit.Id, "Delete:UnitMembership@UnitId");
        return PermissionWrapper.Create(item, canUpdate, canDelete);
      }

      return PermissionWrapper.Create(item, canUpdate, canDelete);
    }
  }
}