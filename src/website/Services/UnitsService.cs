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
