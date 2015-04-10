/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using Kcsara.Database.Model.Units;
  using log4net;

  public interface IUnitsService
  {
    List<UnitSummary> List();
  }

  public class UnitsService : BaseDataService, IUnitsService
  {
    /// <summary>Default Constructor</summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public UnitsService(Func<IKcsarContext> dbFactory, ILog log)
      : base(dbFactory, log)
    {
    }

    protected Expression<Func<UnitRow, UnitSummary>> toDomainSummary = row => new UnitSummary
    {
      Id = row.Id,
      Nickname = row.DisplayName,
      Name = row.LongName
    };

    /// <summary>Get a list of units</summary>
    /// <returns></returns>
    public List<UnitSummary> List()
    {
      using (var db = this.dbFactory())
      {
        return db.Units.OrderBy(f => f.DisplayName).Select(toDomainSummary).ToList();
      }
    }
  }
}
