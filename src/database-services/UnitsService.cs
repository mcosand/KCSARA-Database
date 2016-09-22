﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Units;
using Data = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IUnitsService
  {
    Task<ListPermissionWrapper<Unit>> List();
    Task<ListPermissionWrapper<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate);
    Task<UnitMembership> CreateMembership(UnitMembership membership);
    Task<ListPermissionWrapper<UnitStatusType>> ListStatusTypes(Guid? unitId = null);
    Task<ItemPermissionWrapper<Unit>> Get(Guid id);
  }

  public class UnitsService : IUnitsService
  {
    private readonly Func<Data.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    public UnitsService(Func<Data.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    public async Task<ListPermissionWrapper<Unit>> List()
    {
      using (var db = _dbFactory())
      {
        var list = await db.Units.OrderBy(f => f.DisplayName).Select(f => new Unit
        {
          Id = f.Id,
          Name = f.DisplayName,
          FullName = f.LongName
        }).ToListAsync();

        return new ListPermissionWrapper<Unit>
        {
          C = 1,
          Items = list.Select(f => PermissionWrapper.Create(f, 1, 1))
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
          FullName = f.LongName
        }).SingleOrDefaultAsync(f => f.Id == id);

        return PermissionWrapper.Create(result, 1, 1);
      }
    }

    public async Task<ListPermissionWrapper<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate)
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
          C = 1,
          Items = list.Select(f => PermissionWrapper.Create(f, 1, 1))
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

        return (await ListMemberships(f => f.Id == membershipRow.Id)).Items.Select(f => f.Item).Single();
      }
    }

    public async Task<ListPermissionWrapper<UnitStatusType>> ListStatusTypes(Guid? unitId = null)
    {
      using (var db = _dbFactory())
      {
        IQueryable<Data.UnitStatus> query = db.UnitStatusTypes;
        if (unitId.HasValue) query = query.Where(f => f.Unit.Id == unitId.Value);

        return new ListPermissionWrapper<UnitStatusType>
        {
          C = 1,
          Items = (await query.Select(f => new UnitStatusType
          {
            Id = f.Id,
            Unit = new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
            IsActive = f.IsActive,
            GetsAccount = f.GetsAccount,
            Name = f.StatusName,
            WacLevel = (WacLevel)(int)f.WacLevel
          }).ToListAsync()).Select(f => PermissionWrapper.Create(f, 1, 1))
        };
      }
    }
  }
}
