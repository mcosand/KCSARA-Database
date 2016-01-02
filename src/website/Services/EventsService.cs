/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Models;

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="ModelType"></typeparam>
  public interface IEventsService<ModelType> where ModelType : EventSummary, new()
  {
    EventList List(int? year);

    ModelType Save(ModelType model);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class EventsService<RowType, ModelType> : IEventsService<ModelType>
    where RowType : SarEventRow, new()
    where ModelType : EventSummary, new()
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;

    public EventsService(Func<IKcsarContext> dbFactory, ILog log)
    {
      this.dbFactory = dbFactory;
      this.log = log;
    }

    public EventList List(int? year)
    {
      using (var db = dbFactory())
      {
        IQueryable<SarEventRow> query = db.Events.OfType<RowType>();

        if (year != null)
        {
          var dateStart = new DateTime(year.Value, 1, 1);
          var dateEnd = dateStart.AddYears(1);
          query = query.Where(f => f.StartTime >= dateStart && f.StartTime < dateEnd);
        }

        var result = (from e in query.SelectMany(f => f.Roster).DefaultIfEmpty()
                      group e by 1 into g
                      select new EventList
                      {
                        People = g.DefaultIfEmpty().Select(f => (Guid?)f.Person.Id).Where(f => f != null).Distinct().Count(),
                        Hours = Math.Round(g.Sum(f => SqlFunctions.DateDiff("minute", f.TimeIn, f.TimeOut) / 15.0) ?? 0.0) / 4.0,
                        Miles = g.Sum(f => f.Miles)
                      }).SingleOrDefault();

        result.Events = query
          .OrderBy(f => f.StartTime)
          .Select(f => new
          {
            Id = f.Id,
            Number = f.StateNumber,
            Date = f.StartTime,
            Title = f.Title,
            People = f.Roster.DefaultIfEmpty().Select(g => (Guid?)g.Person.Id).Where(g => g != null).Distinct().Count(),
            Hours = Math.Round(f.Roster.DefaultIfEmpty().Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 15.0) ?? 0.0) / 4.0,
            Miles = f.Roster.DefaultIfEmpty().Sum(g => g.Miles)
          })
          .ToList();

        return result;
      }
    }

    public ModelType Save(ModelType model)
    {
      using (var db = dbFactory())
      {
        RowType row = null;
        if (model.Id == Guid.Empty)
        {
          row = new RowType();
          db.Events.Add(row);
        }
        else
        {
          row = db.Events.OfType<RowType>().SingleOrDefault(f => f.Id == model.Id);
          if (row == null) throw new NotFoundException();
        }

        Dictionary<string, string> errors = new Dictionary<string, string>();

        if (model.Name != row.Title) { row.Title = model.Name; }
        if (string.IsNullOrWhiteSpace(row.Title)) { errors.Add("Name", "Required"); }

        if (model.Location != row.Location) { row.Location = model.Location; }
        if (string.IsNullOrWhiteSpace(row.Location)) { errors.Add("Location", "Required"); }


        if (model.StateNumber != row.StateNumber) { row.StateNumber = model.StateNumber; }

        if (model.Start != row.StartTime) { row.StartTime = model.Start; }
        if (model.Start < Kcsar.Database.Model.Utils.SarEpoch || model.Start > DateTime.Now.AddYears(1)) { errors.Add("Start", "Invalid Date"); }

        if (model.Stop != row.StopTime) { row.StopTime = model.Stop; }
        if (model.Stop != null && model.Stop < Kcsar.Database.Model.Utils.SarEpoch || model.Start > DateTime.Now.AddYears(1)) { errors.Add("Stop", "Invalid Date"); }

        InternalApiSaveProcessModel(model, row, errors);

        if (errors.Count > 0)
        {
          throw new ModelErrorsException(errors);
        }

        db.SaveChanges();

        model.Id = row.Id;
        return model;
      }
    }

    protected virtual void InternalApiSaveProcessModel(ModelType evt, RowType row, Dictionary<string, string> errors)
    {
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class MissionsService : EventsService<MissionRow, Mission>
  {
    public MissionsService(Func<IKcsarContext> dbFactory, ILog log) : base(dbFactory, log)
    { }

    protected override void InternalApiSaveProcessModel(Mission evt, MissionRow row, Dictionary<string, string> errors)
    {
      base.InternalApiSaveProcessModel(evt, row, errors);
      //var allowedTypes = string
      //var invalidTypes = evt.Type.Select(f => f.ToLowerInvariant()).Where()

      var types = string.Join(",", (evt.Type ?? new string[0]).Select(f => f.ToLowerInvariant()));
      if (types != row.MissionType) { row.MissionType = types; }
    }
  }
}
