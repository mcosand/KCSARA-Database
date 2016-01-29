/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Models;

  /// <summary>
  /// 
  /// </summary>
  public interface IUnitsService
  {
    IEnumerable<NameIdPair> List();
    SarUnit GetUnit(Guid id);
    object GetRoster(Guid unitId);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class UnitsService : IUnitsService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public UnitsService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
    }

    public object GetRoster(Guid unitId)
    {
      using (var db = dbFactory())
      {
        var members = db.UnitMemberships.Where(um => um.Unit.Id == unitId && um.EndTime == null);
        members = members.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

        return members.Select(f => new
        {
          Member = new MemberSummary { Id = f.Person.Id, Name = f.Person.LastName + ", " + f.Person.FirstName, Photo = f.Person.PhotoFile, WorkerNumber = f.Person.DEM },
          Status = f.Status.StatusName,
          AsOf = f.Activated
        }).ToList();
      }
    }

    public SarUnit GetUnit(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.Units.Select(f => new SarUnit { Id = f.Id, Name = f.DisplayName }).SingleOrDefault(f => f.Id == id);
      }
    }

    public IEnumerable<NameIdPair> List()
    {
      using (var db = dbFactory())
      {
        return db.Units
          .Select(f => new NameIdPair { Id = f.Id, Name = f.DisplayName })
          .OrderBy(f => f.Name)
          .ToList();
      }
    }
  }
}
