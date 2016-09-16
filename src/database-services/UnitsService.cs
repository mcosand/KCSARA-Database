using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kcsara.Database.Model.Units;
using Sar.Model;
using Sar.Services.Auth;
using Data = Kcsar.Database.Model;

namespace Kcsara.Database.Services
{
  public interface IUnitsService
  {
    Task<IEnumerable<Unit>> List();
    Task<IEnumerable<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate);
    Task<UnitMembership> CreateMembership(UnitMembership membership);
    Task<IEnumerable<UnitStatusType>> ListStatusTypes(Guid? unitId = null);
  }

  public class UnitsService : IUnitsService
  {
    private readonly Func<Data.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IAuthenticatedHost _host;

    public UnitsService(Func<Data.IKcsarContext> dbFactory, IAuthorizationService authSvc, IAuthenticatedHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    public async Task<IEnumerable<Unit>> List()
    {
      using (var db = _dbFactory())
      {
        var list = await db.Units.OrderBy(f => f.DisplayName).Select(f => new Unit
        {
          Id = f.Id,
          Name = f.DisplayName,
          FullName = f.LongName
        }).ToListAsync();

        return list;
      }
    }

    public async Task<IEnumerable<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate)
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
          Member = new NameIdPair
          {
            Id = f.Person.Id,
            Name = f.Person.LastName + ", " + f.Person.FirstName
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
        return list;
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
        var membershipRow = new Data.UnitMembership
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

        return (await ListMemberships(f => f.Id == membershipRow.Id)).Single();
      }
    }

    public async Task<IEnumerable<UnitStatusType>> ListStatusTypes(Guid? unitId = null)
    {
      using (var db = _dbFactory())
      {
        IQueryable<Data.UnitStatus> query = db.UnitStatusTypes;
        if (unitId.HasValue) query = query.Where(f => f.Unit.Id == unitId.Value);

        return (await query.Select(f => new UnitStatusType
        {
          Id = f.Id,
          Unit = new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
          IsActive = f.IsActive,
          GetsAccount = f.GetsAccount,
          Name = f.StatusName
        }).ToListAsync());
      }
    }
  }
}
