using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CsvHelper;
using Sar.Database.Data;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Services.Training;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface ITrainingRecordsService
  {
    Task<List<TrainingStatus>> RequiredTrainingStatusForMember(Guid memberId, DateTimeOffset asOf);
    Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatusForUnit(Guid? unitId, DateTimeOffset asOf);
    Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(Stream dataStream);
    Task<ListPermissionWrapper<TrainingStatus>> RecordsForMember(Guid memberId, DateTimeOffset now);
    Task<ListPermissionWrapper<TrainingRecord>> List(Expression<Func<TrainingRecord, bool>> filter);
    Task<TrainingRecord> SaveAsync(TrainingRecord record);
  }

  public class TrainingRecordsService : ITrainingRecordsService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    /// <summary></summary>
    public TrainingRecordsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    /// <summary></summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task<List<TrainingStatus>> RequiredTrainingStatusForMember(Guid memberId, DateTimeOffset asOf)
    {
      using (var db = _dbFactory())
      {
        var memberQuery = db.Members.Where(f => f.Id == memberId);
        var training = await RequiredTrainingStatus(db, memberQuery, asOf);

        List<TrainingStatus> result;
        if (!training.TryGetValue(memberId, out result))
        {
          throw new Exception("Member Not Found");
        }

        return result;
      }
    }

    /// <summary></summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatusForUnit(Guid? unitId, DateTimeOffset asOf)
    {
      using (var db = _dbFactory())
      {
        var membershipQuery = db.UnitMemberships.Where(f => f.Status.IsActive && f.EndTime == null || f.EndTime > asOf);
        if (unitId.HasValue) membershipQuery = membershipQuery.Where(f => f.Unit.Id == unitId.Value);

        var membersQuery = membershipQuery.Select(f => f.Person).Distinct();

        var training = await RequiredTrainingStatus(db, membersQuery, asOf);
        return training;
      }
    }

    /// <summary></summary>
    /// <param name="db"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    private async Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatus(DB.IKcsarContext db, IQueryable<DB.Member> members, DateTimeOffset asOf)
    {
      // For all the specified members,
      // - Get their required training (if any)
      // - Get their completed training (if any)
      //   - Filter completed training to only required courses
      // - Return the status along with some extra information needed for later
      var query = (from m in members
                   join tr in db.RequiredTraining.Where(f => f.From <= asOf && f.Until > asOf) on m.WacLevel equals tr.WacLevel into group1

                   from required in group1.DefaultIfEmpty()
                   join cta in db.ComputedTrainingAwards on new { required.CourseId, m.Id } equals new { CourseId = cta.Course.Id, cta.Member.Id } into group2

                   from completed in group2.DefaultIfEmpty()

                   select new CompletedCourseInfo
                   {
                     Status = new TrainingStatus
                     {
                       Course = required == null ? null : new NameIdPair { Id = required.CourseId, Name = required.Course.DisplayName },
                       Completed = completed.Completed,
                       Expires = completed.Expiry,
                     },
                     WacDate = m.WacLevelDate,
                     Requirement = required,
                     MemberId = m.Id
                   });

      var grouped = await query.GroupBy(f => f.MemberId).ToListAsync();

      return ComputeTrainingStatus(grouped, asOf);
    }

    private static Dictionary<Guid, List<TrainingStatus>> ComputeTrainingStatus(List<IGrouping<Guid, CompletedCourseInfo>> grouped, DateTimeOffset asOf)
    {
      var withStatus = grouped.Select(f => new MemberAndStatus
      {
        MemberId = f.Key,
        Courses = f
            .GroupBy(g => new WacDateAndCourse { WacDate = g.WacDate, CourseId = g.Status.Course == null ? (Guid?)null : g.Status.Course.Id })
            // in the future there will be multiple "scopes" for required training (federal, state, county, team) and this test will
            // filter out scopes that the member does not belong to or are not passed as arguments to this method
            //.Where(g => true)
            .Select(
              g =>
              {
                var s = g.Select(h => h.Status).FirstOrDefault();

                if (s == null || g.All(h => h.Requirement == null))
                {
                  return s;
                }

                if (s.Completed.HasValue)
                {
                  if (s.Expires.HasValue)
                  {
                    if (s.Expires.Value < asOf)
                    {
                      s.Status = g.Key.WacDate.AddMonths(g.Max(h => h.Requirement.GraceMonths)) > asOf ? ExpirationFlags.ToBeCompleted : ExpirationFlags.Expired;
                    }
                    else // isn't expired
                    {
                      s.Status = ExpirationFlags.NotExpired;
                    }
                  }
                  else // doesn't expire
                  {
                    s.Status = ExpirationFlags.Complete;
                  }
                }
                else // not completed
                {
                  s.Status = g.Key.WacDate.AddMonths(g.Max(h => h.Requirement.GraceMonths)) > asOf ? ExpirationFlags.ToBeCompleted : ExpirationFlags.Missing;
                }
                return s;
              })
      });

      return withStatus.ToDictionary(f => f.MemberId, f => f.Courses.Where(g => g.Course != null).OrderBy(g => g.Course.Name).ToList());
    }

    class WacDateAndCourse
    {
      public DateTimeOffset WacDate { get; set; }
      public Guid? CourseId { get; set; }
    }

    class MemberAndStatus
    {
      public Guid MemberId { get; set; }
      public IEnumerable<TrainingStatus> Courses { get; set; }
    }

    class CompletedCourseInfo
    {
      public TrainingStatus Status { get; set; }
      public DateTimeOffset WacDate { get; set; }
      public DB.TrainingRequired Requirement { get; set; }
      public Guid MemberId { get; set; }
    }

    /// <summary></summary>
    /// <param name="dataStream"></param>
    /// <returns></returns>
    public async Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(Stream dataStream)
    {
      var result = new List<ParsedKcsaraCsv>();

      using (var db = _dbFactory())
      {
        var courses = await db.TrainingCourses.ToDictionaryAsync(f => f.FullName, f => new NameIdPair { Id = f.Id, Name = f.DisplayName });
        using (var reader = new StreamReader(dataStream))
        {
          using (var csv = new CsvParser(reader))
          {
            string[] row;
            row = csv.Read();
            if (row[1] != "Email") throw new InvalidOperationException("Unexpected file format");
            row = csv.Read();
            if (row[0] != "Link to Certificate") throw new InvalidOperationException("Unexpected file format");

            while ((row = csv.Read()) != null)
            {
              string link = csv.Read().FirstOrDefault() ?? string.Empty;

              var entry = new ParsedKcsaraCsv
              {
                Email = row[1],
                Name = row[0],
                Link = link,
                Completed = DateTimeOffset.Parse(row[6])
              };

              NameIdPair course;
              if (!courses.TryGetValue("Core/" + row[3] + " - Written", out course))
              {
                if (!courses.TryGetValue("Core/" + row[3], out course))
                {
                  course = null;
                  entry.Error = "Can't find course " + row[3];
                }
              }
              entry.Course = course;

              var multiple = false;
              string dem = row[2];
              var matches = await db.Members.Where(f => f.ContactNumbers.Any(g => g.Value == entry.Email)).ToListAsync();
              if (matches.Count > 1 && !string.IsNullOrWhiteSpace(dem))
              {
                multiple = true;
                matches = matches.Where(f => f.DEM == dem).ToList();
              }
              else if (matches.Count == 0)
              {
                matches = await db.Members.Where(f => f.DEM == dem && f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null))).ToListAsync();
              }

              if (matches.Count == 1)
              {
                entry.Member = new NameIdPair { Id = matches[0].Id, Name = matches[0].FullName };
              }
              else if (matches.Count > 1 || multiple)
              {
                entry.Error = "Multiple members with email " + entry.Email;
              }
              else
              {
                entry.Error = "No match found for " + entry.Email;
              }
              result.Add(entry);
            }
          }
        }

        if (result.Count > 0)
        {
          DateTimeOffset min = result.Min(f => f.Completed);
          DateTimeOffset max = result.Max(f => f.Completed);
          var awards = db.TrainingAward.Where(f => f.Completed >= min && f.Completed <= max).ToList();

          foreach (var row in result)
          {
            if (!string.IsNullOrWhiteSpace(row.Error)) continue;

            var existing = awards.FirstOrDefault(f => f.Completed == row.Completed && f.Member.Id == row.Member.Id && f.Course.Id == row.Course.Id);
            if (existing != null)
            {
              row.Existing = existing.Id;
            }
          }
        }
      }
      return result;
    }

    public async Task<ListPermissionWrapper<TrainingStatus>> RecordsForMember(Guid memberId, DateTimeOffset asOf)
    {
      using (var db = _dbFactory())
      {
        var memberQuery = db.Members.Where(f => f.Id == memberId);
        var member = memberQuery.Single();

        var wacDate = member.WacLevelDate;

        var courses = await db.TrainingCourses
                                 .ToDictionaryAsync(f => f.Id, f => f);
        var requirements = await db.RequiredTraining
                                    .Where(f => f.From <= asOf && f.Until > asOf && f.WacLevel == member.WacLevel)
                                    .ToDictionaryAsync(f => f.Id, f => f);

        var list = await memberQuery.SelectMany(f => f.ComputedAwards).Select(f => new CompletedCourseInfo
        {
          Status = new TrainingStatus
          {
            Id = f.Id,
            Course = new NameIdPair { Id = f.Course.Id, Name = f.Course.DisplayName },
            Completed = f.Completed,
            Expires = f.Expiry,
            EventId = f.Roster.Training.Id,
            FromRule = f.Rule != null
          },
          WacDate = wacDate,
          MemberId = f.Member.Id,
        })
        .ToListAsync();

        list.ForEach(item =>
        {
          DB.TrainingRequired required;
          if (!requirements.TryGetValue(item.Status.Course.Id, out required))
          {
            required = new DB.TrainingRequired { Course = courses[item.Status.Course.Id], CourseId = item.Status.Course.Id, GraceMonths = 0, JustOnce = false };
          }
          item.Requirement = required;
          item.Status.EventType = item.Status.EventId.HasValue ? "training" : null;
        });

        var items = list.Any()
          ? ComputeTrainingStatus(list.GroupBy(f => f.MemberId).ToList(), asOf)[memberId].Select(f => WrapStatusForMember(memberId, f))
          : new List<ItemPermissionWrapper<TrainingStatus>>();

        return new ListPermissionWrapper<TrainingStatus>
        {
          C = _authz.Authorize(memberId, "Create:TrainingRecord@MemberId"),
          Items = items
        };
      }
    }

    private ItemPermissionWrapper<TrainingStatus> WrapStatusForMember(Guid memberId, TrainingStatus status)
    {
      var canUpdate = false;
      if (!canUpdate && status.EventId.HasValue && status.EventType == "trainings" && _authz.Authorize(status.EventId, "Update:TrainingRoster@TrainingId")) canUpdate = true;
      if (!canUpdate && !status.FromRule && _authz.Authorize(memberId, "Update:TrainingRecord@MemberId")) canUpdate = true;
      return new ItemPermissionWrapper<TrainingStatus>
      {
        U = _authz.Authorize(memberId, "Update:TrainingRecord@MemberId"),
        D = _authz.Authorize(memberId, "Delete:TrainingRecord@MemberId"),
        Item = status
      };
    }

    public async Task<TrainingRecord> SaveAsync(TrainingRecord record)
    {
      using (var db = _dbFactory())
      {
        var match = await db.TrainingAward.FirstOrDefaultAsync(
          f => f.Id != record.Id &&
          f.Member.Id == record.Member.Id &&
          f.Course.Id == record.Course.Id &&
          f.Completed == record.Completed
          );
        if (match != null) throw new DuplicateItemException("TrainingRecord", record.Id.ToString());

        var updater = await ObjectUpdater.CreateUpdater(
          db.TrainingAward,
          record.Id,
          null
          );

        var course = await db.TrainingCourses.FirstOrDefaultAsync(f => f.Id == record.Course.Id);
        updater.Update(f => f.Member, await db.Members.FirstOrDefaultAsync(f => f.Id == record.Member.Id));
        updater.Update(f => f.Course, course);
        updater.Update(f => f.Completed, record.Completed);

        int? courseMonths = updater.Instance.Course.ValidMonths;
        DateTimeOffset? expiration;
        switch (record.ExpirySrc)
        {
          default:
          case "default":
            expiration = course.ValidMonths.HasValue ? record.Completed.AddMonths(course.ValidMonths.Value) : (DateTimeOffset?)null;
            break;
          case "custom":
            expiration = record.Expires;
            break;
          case "never":
            expiration = null;
            break;
        }
        updater.Update(f => f.Expiry, expiration);

        await updater.Persist(db);

        return (await List(f => f.Id == updater.Instance.Id)).Items.Single().Item;
      }
    }

    public async Task<ListPermissionWrapper<TrainingRecord>> List(Expression<Func<TrainingRecord, bool>> filter)
    {
      using (var db = _dbFactory())
      {
        var list = await ProjectTrainingRecordRows(db, db.TrainingAward)
        .Where(filter)
        .ToListAsync();

        return new ListPermissionWrapper<TrainingRecord>
        {
          C = _authz.CanCreate<TrainingRecord>(),
          Items = list.Select(f => _authz.Wrap(f))
        };
      }
    }

    internal static IQueryable<TrainingRecord> ProjectTrainingRecordRows(DB.IKcsarContext db, IQueryable<DB.TrainingAward> query)
    {
      return query.Select(f => new TrainingRecord
      {
        Id = f.Id,
        Member = new NameIdPair { Id = f.Member.Id, Name = f.Member.FirstName + " " + f.Member.LastName },
        Course = new NameIdPair { Id = f.Course.Id, Name = f.Course.DisplayName },
        Completed = f.Completed,
        Expires = f.Expiry,
        Comments = f.metadata,
        Source = (f.Roster != null) ? "roster" : "direct",
        ReferenceId = (from roster in db.TrainingRosters where roster.Id == f.Roster.Id select (Guid?)roster.Training.Id).FirstOrDefault() ?? f.Id,
      })
      .OrderByDescending(f => f.Completed)
      .ThenBy(f => f.Source);
    }
  }
}
