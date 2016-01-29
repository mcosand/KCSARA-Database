/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Linq;
  using System.Threading.Tasks;
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
    Task<object> ListRequired(Guid memberId);
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
    private readonly List<RequiredScope> requiredScopes;

    public TrainingService(Func<IKcsarContext> dbFactory, ILog log) : this(dbFactory, log, DefaultRequiredScopes)
    {
    }

    internal TrainingService(Func<IKcsarContext> dbFactory, ILog log, List<RequiredScope> requiredScopes)
    {
      this.dbFactory = dbFactory;
      this.log = log;
      this.requiredScopes = requiredScopes;
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

    // TODO: Figure out how to store this in the database
    private static readonly List<RequiredScope> DefaultRequiredScopes = new List<RequiredScope>
    {
      new RequiredScope
      {
        Name = "Washington State",
        Courses = new List<RequiredCourse>
        {
          new RequiredCourse { CourseId = new Guid("6D826AB0-0802-47AA-9C4E-833B39F863EC"), MinWacLevel = WacLevel.Novice }, // ICS-100
          new RequiredCourse { CourseId = new Guid("7F104382-FC66-4A8A-8A38-A72DEF66716A"), MinWacLevel = WacLevel.Novice }, // ICS-700
          new RequiredCourse { CourseId = new Guid("ED198D53-7970-425A-A13E-644174801F10"), MinWacLevel = WacLevel.Novice }, // CPR
          new RequiredCourse { CourseId = new Guid("F1BB9A3C-0512-4B36-BE2F-FEDBDAFAE69B"), MinWacLevel = WacLevel.None } // First Aid
        }
      },
      new RequiredScope
      {
        Name = "King County Sheriff",
        Courses  =new List<RequiredCourse>
        {
          new RequiredCourse { CourseId = new Guid("17C89F0F-3C54-43DC-AC63-B7DC42F30EC4"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair>
          {
            new NameIdPair(new Guid("17C89F0F-3C54-43DC-AC63-B7DC42F30EC4"), "Written")
          } }, // Clue Awareness
          new RequiredCourse { CourseId = new Guid("29790979-EF97-40A3-B5DD-412E95766716"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("4524694F-823E-4483-962D-4E1D14B9955B"), "Written"),
            new NameIdPair(new Guid("DE1EC683-8A0B-4856-BAEB-33250CB01903"), "Classroom")
          } }, // Crime
          new RequiredCourse { CourseId = new Guid("A7B432DC-1AEA-4864-B121-A5F932F62B58"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair>
          {
            new NameIdPair(new Guid("A7B432DC-1AEA-4864-B121-A5F932F62B58"), "Written")
          } }, // Fitness
          new RequiredCourse { CourseId = new Guid("81770EA9-3761-4943-8E0A-E0CB4DC08218"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("991AED43-0C9F-4570-9930-FE374C873EF1"), "Written"),
            new NameIdPair(new Guid("6B4D61C5-67F3-4460-A692-EFB028AFF10D"), "Performance")
          } }, // GPS
          new RequiredCourse { CourseId = new Guid("1A5F6581-1998-439A-A831-7158990FEC4D"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("98DB42F7-5983-45ED-8DDF-FCA0123542E8"), "Written"),
            new NameIdPair(new Guid("B82DC502-4A73-4EBC-96BA-84FBA97FE303"), "Classroom")
          } }, // Helo
          new RequiredCourse { CourseId = new Guid("DA7ECFB5-EC44-4EA3-91D8-9CC345DF680E"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("2115A5E4-CAB5-42E5-A4F5-15998B00CF7D"), "Written"),
            new NameIdPair(new Guid("E110D9AA-005F-42A1-B725-4F2217357944"), "Classroom")
          } }, // Legal
          new RequiredCourse { CourseId = new Guid("7CD43291-A0BA-44FD-AC50-DCA4FB6F12A5"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair>
          {
            new NameIdPair(new Guid("7CD43291-A0BA-44FD-AC50-DCA4FB6F12A5"), "Written")
          } }, // Management
          new RequiredCourse { CourseId = new Guid("81FAE176-E577-41E3-9072-6B2FDFC62138"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("D2BF552D-5A0F-435E-A62A-89BB5E080383"), "Written"),
            new NameIdPair(new Guid("D85BBEA6-A98F-4C10-958F-6D6A048F0F54"), "Performance")
          } }, // Nav
          new RequiredCourse { CourseId = new Guid("75146EBE-41C8-42C2-AEDB-AD33DDD050E3"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("88956F48-6BB5-4277-A3A9-17D4ADFFB161"), "Written"),
            new NameIdPair(new Guid("DFBD2660-5E25-4966-B3A1-1E1462B110B9"), "Performance")
          } }, // Radio
          new RequiredCourse { CourseId = new Guid("FF0A7AB6-7F53-409C-AABE-F99244D6125D"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("4DEA67ED-9EDF-41F1-ADCF-BF5CDE21633D"), "Written"),
            new NameIdPair(new Guid("A38FE38F-197B-4A5C-8B37-70D2EA273E3D"), "Performance")
          } }, // Rescue
          new RequiredCourse { CourseId = new Guid("0F053FFF-5696-4713-895A-C8F4A65E3135"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("661A47CF-8BC9-4616-9799-409A32E09A15"), "Written"),
            new NameIdPair(new Guid("4000D347-ED32-426A-AF94-6FC30DBE0F60"), "Performance")
          } }, // Searcher Safety
          new RequiredCourse { CourseId = new Guid("0914998A-A4F5-4840-886E-ED26826334FE"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("29535B63-3BE8-49BA-9802-AC656DEFE6E8"), "Written"),
            new NameIdPair(new Guid("EF85DB36-4BF3-42BE-A1F7-249B3FEC738F"), "Performance")
          } }, // Search Techniques
          new RequiredCourse { CourseId = new Guid("FDC85C50-0FCE-459D-9216-6D722FDB7882"), MinWacLevel = WacLevel.Support, Parts = new List<NameIdPair> {
            new NameIdPair(new Guid("4639CA71-B9A8-4FD0-8077-75845D20A9A5"), "Written"),
            new NameIdPair(new Guid("CFB533AB-A1A9-4561-8DCF-A8BA807739D4"), "Performance")
          } } // Survival
        }
      }
    };

    public async Task<object> ListRequired(Guid memberId)
    {
      DateTime now = DateTime.Now;

      using (var db = dbFactory())
      {
        var courses = await db.TrainingCourses.ToDictionaryAsync(f => f.Id, f => f.DisplayName);
        var completed = await db.ComputedTrainingAwards.Include(f => f.Course).Where(f => f.MemberId == memberId).ToDictionaryAsync(
          f => f.CourseId,
          f => f);

        return ComputedRequiredTrainingStatus(memberId, now, courses, completed);
      }
    }

    internal List<ScopedTrainingStatus> ComputedRequiredTrainingStatus(Guid memberId, DateTime now, Dictionary<Guid, string> courses, Dictionary<Guid, ComputedTrainingAwardRow> completed)
    {
      var result = new List<ScopedTrainingStatus>();
      foreach (var scope in requiredScopes)
      {
        if (!IsInTrainingScope(memberId, scope.Name)) continue;

        var resultScope = new ScopedTrainingStatus(scope.Name);
        result.Add(resultScope);

        foreach (var course in scope.Courses)
        {
          ComputedTrainingAwardRow row;
          Models.Training.TrainingStatus status;
          if (course.Parts.Count > 0)
          {
            var compStatus = new Models.Training.CompositeTrainingStatus() { Status = Models.Training.ExpirationFlags.Missing };
            status = compStatus;

            if (completed.TryGetValue(course.CourseId, out row))
            {
              status.Completed = row.Completed;
              status.Expires = row.Expiry;
              status.Status = GetExpirationFlags(row.Expiry, now);
            }
            DateTime? minPartExpiry = null;
            foreach (var part in course.Parts)
            {
              var partStatus = new Models.Training.TrainingStatus
              {
                Course = part,
                Completed = status.Completed,
                Expires = status.Expires,
                Status = status.Status
              };
              compStatus.Parts.Add(partStatus);
              if (completed.TryGetValue(part.Id, out row) && (status.Completed == null || row.Expiry > status.Expires))
              {
                partStatus.Completed = row.Completed;
                partStatus.Expires = row.Expiry;
                partStatus.Status = GetExpirationFlags(partStatus.Expires, now);
              }
              minPartExpiry = partStatus.Expires;
            }
            //if (compStatus.Completed == null || minPartExpiry > compStatus.Expires)
            //{
            //  compStatus.Expires = minPartExpiry;
            //  compStatus.Status = GetExpirationFlags(compStatus.Expires, now);
            //}
          }
          else if (completed.TryGetValue(course.CourseId, out row))
          {
            status = new Models.Training.TrainingStatus
            {
              Completed = row.Completed,
              Expires = row.Expiry,
              Status = GetExpirationFlags(row.Expiry, now)
            };
          }
          else
          {
            status = new Models.Training.TrainingStatus { Status = Models.Training.ExpirationFlags.Missing };
          }
          status.Course = new NameIdPair { Id = course.CourseId, Name = courses[course.CourseId] };
          resultScope.Courses.Add(status);
        }
      }
      return result;
    }

    private bool IsInTrainingScope(Guid memberId, string scopeName)
    {
      return true;
    }

    private static Models.Training.ExpirationFlags GetExpirationFlags(DateTime? expiry, DateTime now)
    {
      return expiry == null ? Models.Training.ExpirationFlags.Complete
                            : (expiry >= now) ? Models.Training.ExpirationFlags.NotExpired
                                              : Models.Training.ExpirationFlags.Expired;
    }

    internal class RequiredScope
    {
      public string Name { get; set; }
      public List<RequiredCourse> Courses { get; set; }
    }

    internal class RequiredCourse
    {
      public RequiredCourse()
      {
        Parts = new List<NameIdPair>();
      }
      public Guid CourseId { get; set; }
      public WacLevel MinWacLevel { get; set; }
      public List<NameIdPair> Parts { get; set; }
    }
  }
}
