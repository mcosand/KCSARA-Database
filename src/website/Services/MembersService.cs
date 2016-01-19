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
    object Contacts(Guid id);
    object Addresses(Guid id);

    IEnumerable<MemberSummary> Search(string query);
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
        return SummariesWithUnits(db.Members.Where(f => f.Id == id)).FirstOrDefault();
      }
    }

    public object Contacts(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.PersonContact.Where(f => f.Person.Id == id)
          .Select(f => new
          {
            Value = f.Value,
            Type = f.Type,
            SubType = f.Subtype,
            Priority = f.Priority
          })
          .OrderBy(f => f.Type)
          .ThenBy(f => f.SubType)
          .ToArray();
      }
    }

    public object Addresses(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.PersonAddress.Where(f => f.Person.Id == id)
          .Select(f => new
          {
            f.Street,
            f.City,
            f.State,
            f.Zip,
            Type = f.InternalType.Trim(),
          })
          .OrderBy(f => f.Type)
          .ToArray();
      }
    }

    public IEnumerable<MemberSummary> Search(string query)
    {
      using (var db = dbFactory())
      {
        DateTime now = DateTime.Now;
        DateTime last12Months = now.AddMonths(-12);

        return
            SummariesWithUnits(
            db.Members.Where(f => (f.FirstName + " " + f.LastName).StartsWith(query)
                               || (f.LastName + ", " + f.FirstName).StartsWith(query)
                               || (f.DEM.StartsWith(query) || f.DEM.StartsWith("SR" + query)))
            .OrderByDescending(f => f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null || g.EndTime > now)))
            .ThenByDescending(f => f.MissionRosters.Count(g => g.TimeIn > last12Months))
            .ThenByDescending(f => f.MissionRosters.Count())
            .ThenBy(f => f.LastName)
            .ThenBy(f => f.FirstName)
            .ThenBy(f => f.Id))
            .ToArray();
      }
    }

    private IEnumerable<MemberSummary> SummariesWithUnits(IQueryable<Member> query)
    {
      DateTime cutoff = DateTime.Now;
      return query
        .Select(f => new {
          Member = f,
          Units = f.Memberships
                    .Where(g => (g.EndTime == null || g.EndTime > cutoff) && g.Status.IsActive)
                    .Select(g => g.Unit)
                    .Select(g => new NameIdPair { Id = g.Id, Name = g.DisplayName }).Distinct()
        })
        .AsEnumerable()
        .Select(f => new MemberSummary
        {
          Name = f.Member.FullName,
          WorkerNumber = f.Member.DEM,
          Id = f.Member.Id,
          Units = f.Units.ToArray(),
          Photo = f.Member.PhotoFile
        });
    }

  }
}
