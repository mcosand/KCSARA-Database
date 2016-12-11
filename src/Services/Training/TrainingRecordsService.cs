using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Kcsara.Database.Web.Model;
using Kcsara.Database.Web.Model.Training;
using Sar;
using Data = Kcsar.Database.Model;

namespace Kcsara.Database.Web.Services.Training
{
  public interface ITrainingRecordsService
  {
    Task<List<TrainingStatus>> RequiredTrainingStatusForMember(Guid memberId, DateTime asOf);
    Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatusForUnit(Guid? unitId, DateTime asOf);
    Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(Stream dataStream);
  }

  public class TrainingRecordsService : ITrainingRecordsService
  {
    private readonly Func<Data.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    /// <summary></summary>
    public TrainingRecordsService(Func<Data.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    /// <summary></summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task<List<TrainingStatus>> RequiredTrainingStatusForMember(Guid memberId, DateTime asOf)
    {
      if (!await _authz.AuthorizeAsync(_host.User, memberId, "Read:TrainingRecord@MemberId")) throw new AuthorizationException();
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
    public async Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatusForUnit(Guid? unitId, DateTime asOf)
    {
      if (!await _authz.AuthorizeAsync(_host.User, unitId, "Read:TrainingRecord@Unit")) throw new AuthorizationException();
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
    private async Task<Dictionary<Guid, List<TrainingStatus>>> RequiredTrainingStatus(Data.IKcsarContext db, IQueryable<Data.Member> members, DateTime asOf)
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

                   select new
                   {
                     Status = new TrainingStatus
                     {
                       Course = required == null ? null : new NameIdPair { Id = required.CourseId, Name = required.Course.DisplayName },
                       Completed = completed.Completed,
                       Expires = completed.Expiry
                     },
                     WacDate = m.WacLevelDate,
                     Requirement = required,
                     MemberId = m.Id
                   });

      var grouped = await query.GroupBy(f => f.MemberId).ToListAsync();

      var withStatus = grouped.Select(f => new
      {
        MemberId = f.Key,
        Courses = f
            .GroupBy(g => new { WacDate = g.WacDate, CourseId = g.Status.Course == null ? (Guid?)null : g.Status.Course.Id })
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
                      s.Status = g.Key.WacDate.AddMonths(g.Max(h => h.Requirement.GraceMonths)) > asOf ? Model.Training.ExpirationFlags.ToBeCompleted : Model.Training.ExpirationFlags.Expired;
                    }
                    else // isn't expired
                    {
                      s.Status = Model.Training.ExpirationFlags.NotExpired;
                    }
                  }
                  else // doesn't expire
                  {
                    s.Status = Model.Training.ExpirationFlags.Complete;
                  }
                }
                else // not completed
                {
                  s.Status = g.Key.WacDate.AddMonths(g.Max(h => h.Requirement.GraceMonths)) > asOf ? Model.Training.ExpirationFlags.ToBeCompleted : Model.Training.ExpirationFlags.Missing;
                }
                return s;
              })
      });

      return withStatus.ToDictionary(f => f.MemberId, f => f.Courses.Where(g => g.Course != null).OrderBy(g => g.Course.Name).ToList());
    }

    /// <summary></summary>
    /// <param name="dataStream"></param>
    /// <returns></returns>
    public async Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(Stream dataStream)
    {
      if (!await _authz.AuthorizeAsync(_host.User, null, "Read:TrainingRecord")) throw new AuthorizationException();

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
                Completed = DateTime.Parse(row[6])
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
          DateTime min = result.Min(f => f.Completed);
          DateTime max = result.Max(f => f.Completed);
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
  }
}
