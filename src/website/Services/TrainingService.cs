/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Models;
  using Models.Training;

  /// <summary>
  /// 
  /// </summary>
  public interface ITrainingService
  {
    IEnumerable<TrainingRecord> ListRecords(Func<TrainingRecord, bool> where, bool computed = false);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class TrainingService : ITrainingService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public TrainingService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
    }

    public IEnumerable<TrainingRecord> ListRecords(Func<TrainingRecord, bool> where, bool computed = false)
    {
      using (var db = dbFactory())
      {
        return (computed ? (IQueryable<ITrainingAward>)db.ComputedTrainingAwards : (IQueryable<ITrainingAward>)db.TrainingAward)
          .Select(f => new TrainingRecord
          {
            Id = f.Id,
            Course = new NameIdPair { Id = f.Course.Id, Name = f.Course.DisplayName },
            Member = new NameIdPair { Id = f.Member.Id, Name = f.Member.FirstName + " " + f.Member.LastName },
            Completed = f.Completed,
            Expires = f.Expiry,
            Event = f.RosterId.HasValue ? new NameIdPair { Id = f.RosterEntry.Event.Id, Name = f.RosterEntry.Event.Title } : null
          })
         .Where(where)
         .OrderBy(f => f.Completed)
         .ThenBy(f => f.Course.Name)
         .ToList();
      }
    }
  }
}
