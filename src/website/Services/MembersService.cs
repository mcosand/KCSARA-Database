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
  public interface IMembersService
  {
    MemberSummary GetMember(Guid id);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class MembersService : IMembersService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public MembersService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
    }

    public MemberSummary GetMember(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.Members.Select(f => new MemberSummary
        {
          Id = f.Id,
          WorkerNumber = f.DEM,
          Name = f.LastName + ", " + f.FirstName,
        }).SingleOrDefault(f => f.Id == id);
      }
    }
  }
}
