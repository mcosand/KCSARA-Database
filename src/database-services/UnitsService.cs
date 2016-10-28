using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using log4net;
using Sar.Database.Api.Extensions;
using Sar.Database.Data;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Units;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IUnitsService
  {
    Task<ListPermissionWrapper<Unit>> List();
    Task<ListPermissionWrapper<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate, bool canCreate);
    Task<UnitMembership> CreateMembership(UnitMembership membership);
    Task<ListPermissionWrapper<UnitStatusType>> ListStatusTypes(Guid? unitId = null);
    Task<ItemPermissionWrapper<Unit>> Get(Guid id);
    Task DeleteStatusType(Guid statusTypeId);
    Task<UnitStatusType> SaveStatusType(UnitStatusType statusType);
    Task<UnitReportInfo[]> ListReports(Guid unitId);
    Task<Unit> Save(Unit unit);
    Task Delete(Guid unitId);
  }

  public class UnitsService : IUnitsService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly IUsersService _users;
    private readonly ILog _log;
    private readonly IHost _host;
    private readonly IExtensionProvider _extensions;
    private readonly IAuthorizationService _authz;

    public UnitsService(Func<DB.IKcsarContext> dbFactory, IUsersService users, IExtensionProvider extensions, IAuthorizationService authz, IHost host, ILog log)
    {
      _dbFactory = dbFactory;
      _users = users;
      _extensions = extensions;
      _authz = authz;
      _log = log;
      _host = host;
    }

    #region CRUD
    public async Task<ListPermissionWrapper<Unit>> List()
    {
      using (var db = _dbFactory())
      {
        var list = await db.Units.OrderBy(f => f.DisplayName).Select(f => new Unit
        {
          Id = f.Id,
          Name = f.DisplayName,
          FullName = f.LongName,
          County = f.County
        }).ToListAsync();

        return new ListPermissionWrapper<Unit>
        {
          C = _authz.CanCreate<Unit>(),
          Items = list.Select(f => _authz.Wrap(f))
        };
      }
    }

    public async Task<ItemPermissionWrapper<Unit>> Get(Guid id)
    {
      using (var db = _dbFactory())
      {
        var result = await db.Units.Select(f => new Unit
        {
          Id = f.Id,
          Name = f.DisplayName,
          FullName = f.LongName,
          County = f.County
        }).SingleOrDefaultAsync(f => f.Id == id);

        return _authz.Wrap(result);
      }
    }


    public async Task<Unit> Save(Unit unit)
    {
      using (var db = _dbFactory())
      {
        var match = await db.Units.FirstOrDefaultAsync(
          f => f.Id != unit.Id &&
          f.DisplayName == unit.Name);
        if (match != null) throw new DuplicateItemException("Unit", unit.Id.ToString());

        var updater = await ObjectUpdater.CreateUpdater(
          db.Units,
          unit.Id,
          null);

        updater.Update(f => f.DisplayName, unit.Name);
        updater.Update(f => f.LongName, unit.FullName);
        updater.Update(f => f.County, unit.County);

        await updater.Persist(db);

        return (await List()).Items.Single(f => f.Item.Id == updater.Instance.Id).Item;
      }
    }

    public async Task Delete(Guid unitId)
    {
      using (var db = _dbFactory())
      {
        var unit = await db.Units.FirstOrDefaultAsync(f => f.Id == unitId);

        if (unit == null) throw new NotFoundException("not found", "Unit", unitId.ToString());
        db.Units.Remove(unit);
        await db.SaveChangesAsync();
      }
    }
    #endregion

    public async Task<ListPermissionWrapper<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate, bool canCreate)
    {
      using (var db = _dbFactory())
      {
        var query = db.UnitMemberships.Select(f => new UnitMembership
        {
          Id = f.Id,
          Unit = new NameIdPair
          {
            Id = f.Unit.Id,
            Name = f.Unit.DisplayName
          },
          Member = new MemberSummary
          {
            Id = f.Person.Id,
            Name = f.Person.FirstName + " " + f.Person.LastName,
            WorkerNumber = f.Person.DEM,
            Photo = f.Person.PhotoFile
          },
          IsActive = f.Status.IsActive,
          Status = f.Status.StatusName,
          Start = f.Activated,
          End = f.EndTime
        });

        if (predicate != null)
        {
          query = query.Where(predicate);
        }

        var list = await query.OrderBy(f => f.Member.Name).ThenBy(f => f.Unit.Name).ToListAsync();
        return new ListPermissionWrapper<UnitMembership>
        {
          C = canCreate,
          Items = list.Select(f => _authz.Wrap(f))
        };
      }
    }

    public async Task<UnitMembership> CreateMembership(UnitMembership membership)
    {
      if (membership == null) throw new ArgumentNullException(nameof(membership));
      if (membership.Unit == null || membership.Unit.Id == Guid.Empty) throw new ArgumentException("unit.id is required");
      if (membership.Member == null || membership.Member.Id == Guid.Empty) throw new ArgumentException("unit.id is required");

      using (var db = _dbFactory())
      {
        var status = await db.UnitStatusTypes.FirstOrDefaultAsync(f => f.Unit.Id == membership.Unit.Id && f.StatusName == membership.Status);
        if (status == null) throw new ArgumentException("status " + membership.Status + " is unknown");
        var membershipRow = new DB.UnitMembership
        {
          UnitId = membership.Unit.Id,
          PersonId = membership.Member.Id,
          StatusId = status.Id,
          Activated = membership.Start,
          EndTime = membership.End,
          // For logging because lazy-load isn't loading
          Unit = db.Units.Single(f => f.Id == membership.Unit.Id),
          Person = db.Members.Single(f => f.Id == membership.Member.Id),
          Status = status
        };
        db.UnitMemberships.Add(membershipRow);
        await db.SaveChangesAsync();

        return (await ListMemberships(f => f.Id == membershipRow.Id, false)).Items.Select(f => f.Item).Single();
      }
    }

    #region Status Types
    public async Task<ListPermissionWrapper<UnitStatusType>> ListStatusTypes(Guid? unitId = null)
    {
      using (var db = _dbFactory())
      {
        IQueryable<DB.UnitStatus> query = db.UnitStatusTypes;
        if (unitId.HasValue) query = query.Where(f => f.Unit.Id == unitId.Value);

        return new ListPermissionWrapper<UnitStatusType>
        {
          C = _authz.CanCreateStatusForUnit(unitId),
          Items = (await query.Select(f => new UnitStatusType
          {
            Id = f.Id,
            Unit = new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
            IsActive = f.IsActive,
            GetsAccount = f.GetsAccount,
            Name = f.StatusName,
            WacLevel = (WacLevel)(int)f.WacLevel
          }).ToListAsync()).Select(f => _authz.Wrap(f))
        };
      }
    }

    public async Task<UnitStatusType> SaveStatusType(UnitStatusType statusType)
    {
      using (var db = _dbFactory())
      {
        var match = await db.UnitStatusTypes.FirstOrDefaultAsync(
          f => f.Id != statusType.Id &&
          f.UnitId == statusType.Unit.Id &&
          f.StatusName == statusType.Name);
        if (match != null) throw new DuplicateItemException("UnitStatusType", statusType.Id.ToString());

        var updater = await ObjectUpdater.CreateUpdater(
          db.UnitStatusTypes,
          statusType.Id,
          f => {
            f.UnitId = statusType.Unit.Id;
            f.Unit = db.Units.Single(g => g.Id == f.UnitId);
          });

        updater.Update(f => f.StatusName, statusType.Name);
        updater.Update(f => f.WacLevel, statusType.WacLevel.FromModel());
        updater.Update(f => f.IsActive, statusType.IsActive);
        updater.Update(f => f.GetsAccount, statusType.GetsAccount);

        await updater.Persist(db);

        return (await ListStatusTypes()).Items.Single(f => f.Item.Id == updater.Instance.Id).Item;
      }
    }

    public async Task DeleteStatusType(Guid statusTypeId)
    {
      using (var db = _dbFactory())
      {
        var status = await db.UnitStatusTypes.FirstOrDefaultAsync(f => f.Id == statusTypeId);

        if (status == null) throw new NotFoundException("not found", "UnitStatusType", statusTypeId.ToString());
        db.UnitStatusTypes.Remove(status);
        await db.SaveChangesAsync();
      }
    }
    #endregion

    public async Task<UnitReportInfo[]> ListReports(Guid unitId)
    {
      using (var db = _dbFactory())
      {
        var unit = await db.Units.FirstOrDefaultAsync(f => f.Id == unitId);
        if (unit == null) throw new NotFoundException("Not found", "Unit", unitId.ToString());

        var reportProvider = _extensions.For<IUnitReports>(unit);
        return reportProvider != null ? reportProvider.ListReports() : new UnitReportInfo[0];
      }
    }
  }
}
