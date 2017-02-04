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
  public class TrainingCoursesService : ITrainingCoursesService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly IAuthorizationService _authz;
    private readonly IHost _host;

    /// <summary></summary>
    public TrainingCoursesService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authSvc, IHost host)
    {
      _dbFactory = dbFactory;
      _authz = authSvc;
      _host = host;
    }

    public async Task<TrainingCourse> SaveAsync(TrainingCourse course)
    {
      using (var db = _dbFactory())
      {
        var match = await db.TrainingCourses.FirstOrDefaultAsync(
          f => f.Id != course.Id &&
          f.DisplayName == course.Name
          );
        if (match != null) throw new DuplicateItemException("TrainingCourse", course.Id.ToString());

        var updater = await ObjectUpdater.CreateUpdater(
          db.TrainingCourses,
          course.Id,
          null
          );

        updater.Update(f => f.DisplayName, course.Name);
        updater.Update(f => f.FullName, course.FullName);
        updater.Update(f => f.Categories, course.Category ?? "other");
        updater.Update(f => f.ValidMonths, course.ValidMonths > 0 ? course.ValidMonths : null);
        updater.Update(f => f.Unit, null);
        updater.Update(f => f.UnitId, course.Unit?.Id);
        await updater.Persist(db);

        return (await List(f => f.Id == updater.Instance.Id)).Items.Single().Item;
      }
    }

    public async Task<ListPermissionWrapper<TrainingCourse>> List(Expression<Func<TrainingCourse, bool>> filter = null)
    {
      filter = filter ?? (f => true);
      using (var db = _dbFactory())
      {
        var list = await db.TrainingCourses
          .Select(f => new TrainingCourse
          {
            Id = f.Id,
            Name = f.DisplayName
          })
        .Where(filter)
        .OrderBy(f => f.Name)
        .ToListAsync();

        return new ListPermissionWrapper<TrainingCourse>
        {
          C = _authz.CanCreate<TrainingCourse>(),
          Items = list.Select(f => _authz.Wrap(f))
        };
      }
    }

    public async Task<ItemPermissionWrapper<TrainingCourse>> GetAsync(Guid courseId)
    {
      using (var db = _dbFactory())
      {
        var row = await db.TrainingCourses.Select(f => new
        {
          Id = f.Id,
          Name = f.DisplayName,
          FullName = f.FullName,
          Unit = f.Unit == null ? null : new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
          Category = f.Categories,
          ValidMonths = f.ValidMonths,
          PrerequisiteText = f.PrerequisiteText
        }).SingleOrDefaultAsync(f => f.Id == courseId);

        var courseList = (await List()).Items.ToDictionary(f => f.Item.Id, f => f.Item.Name);

        if (row == null) throw new NotFoundException("Course not found", "TrainingCourse", Convert.ToString(courseId));

        var result = new TrainingCourse
        {
          Id = row.Id,
          Name = row.Name,
          FullName = row.FullName,
          Unit = row.Unit,
          Category = row.Category,
          ValidMonths = row.ValidMonths > 0 ? row.ValidMonths : null,
          Prerequisites = ParsePrerequisiteText(row.PrerequisiteText, courseList)
        };

        return _authz.Wrap(result);
      }
    }

    public async Task<List<TrainingRecord>> ListRoster(Guid courseId)
    {
      using (var db = _dbFactory())
      {
        var membersQuery = MembersService.GetActiveMemberQuery(db);
        var awards = db.TrainingAward.Where(f => f.Course.Id == courseId);
        var records = membersQuery.Join(awards, f => f.Id, f => f.MemberId, (member, record) => record );

        return await TrainingRecordsService.ProjectTrainingRecordRows(db, records)
          .ToListAsync();
      }
    }

    public async Task<object> GetCourseStats(Guid courseId)
    {
      using (var db = _dbFactory())
      {
        var membersQuery = MembersService.GetActiveMemberQuery(db);
        var awards = db.TrainingAward.Where(f => f.Course.Id == courseId);
        var records = membersQuery.Join(awards, f => f.Id, f => f.MemberId, (member, record) => record);

        var now = DateTimeOffset.UtcNow;
        var recent = DateTimeOffset.UtcNow.AddMonths(-3);
        var future = DateTimeOffset.UtcNow.AddMonths(3);

        var result = new
        {
          Current = await records.Where(f => f.Expiry == null || f.Expiry > now).CountAsync(),
          Recent = await records.Where(f => f.Expiry < now && f.Expiry > recent).CountAsync(),
          Near = await records.Where(f => f.Expiry > now && f.Expiry < future).CountAsync()
        };

        return new
        {
          Counts = result
        };
      }
    }

    private static NameIdPair[] ParsePrerequisiteText(string preText, Dictionary<Guid, string> courseList)
    {
      if (string.IsNullOrWhiteSpace(preText)) return new NameIdPair[0];

      return preText.Split('+').Select(f =>
      {
        var preReqText = f;
        if (string.Equals(preReqText, "background", StringComparison.OrdinalIgnoreCase))
        {
          return new NameIdPair { Id = Guid.Empty, Name = "Background check" };
        }

        Guid preReqId = Guid.Empty;
        string courseName = null;
        if (Guid.TryParse(preReqText, out preReqId) && courseList.TryGetValue(preReqId, out courseName))
        {
          return new NameIdPair { Id = preReqId, Name = courseName };
        }
        //        
        return new NameIdPair { Id = Guid.Empty, Name = "Unknown" };
      }).ToArray();
    }

    public async Task DeleteAsync(Guid courseId)
    {
      using (var db = _dbFactory())
      {
        var course = await db.TrainingCourses.FirstOrDefaultAsync(f => f.Id == courseId);

        if (course == null) throw new NotFoundException("not found", "Unit", courseId.ToString());
        db.TrainingCourses.Remove(course);
        await db.SaveChangesAsync();
      }
    }
  }

  public interface ITrainingCoursesService
  {
    Task<ListPermissionWrapper<TrainingCourse>> List(Expression<Func<TrainingCourse, bool>> filter = null);
    Task<TrainingCourse> SaveAsync(TrainingCourse course);
    Task DeleteAsync(Guid courseId);
    Task<ItemPermissionWrapper<TrainingCourse>> GetAsync(Guid courseId);
    Task<object> GetCourseStats(Guid courseId);
    Task<List<TrainingRecord>> ListRoster(Guid courseId);
  }
}
