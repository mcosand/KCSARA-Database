/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Linq;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using Kcsara.Database.Model;
  using Kcsara.Database.Model.Training;
  using log4net;

  public interface ITrainingService
  {
    CompositeTrainingStatus GetMemberStatus(string section, Guid memberId, DateTime? when = null);
    TrainingRecord[] ListComputedMemberRecords(Guid memberId, DateTime? when = null);
  }

  public class TrainingService : BaseDataService, ITrainingService
  {
    /// <summary>Default Constructor</summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public TrainingService(Func<IKcsarContext> dbFactory, ILog log)
      : base(dbFactory, log)
    {
    }

    public CompositeTrainingStatus GetMemberStatus(string section, Guid memberId, DateTime? when = null)
    {
      when = when ?? DateTime.Now;
      IEnumerable<TrainingCourseRow> courses = null;
      using (var db = this.dbFactory())
      {
        if (string.IsNullOrWhiteSpace(section) || section == "Core Competencies")
        {
          courses = GetCoreCompetencyCourses(db);
        }
        return ComputeCompositeTraining(db.Members.Single(f => f.Id == memberId), db.ComputedTrainingRecords.Where(f => f.MemberId == memberId).ToList(), courses, when.Value);
      }
    }

    public TrainingRecord[] ListComputedMemberRecords(Guid memberId, DateTime? when = null)
    {
      using (var db = this.dbFactory())
      {
        IQueryable<ComputedTrainingRecordRow> rows = when.HasValue ? ComputeForMember(memberId, when.Value) : db.ComputedTrainingRecords.Where(f => f.MemberId == memberId);

        return rows.Select(toDomain).OrderByDescending(f => f.Completed).ToArray();
      }
    }



    public static Expression<Func<ComputedTrainingRecordRow, TrainingRecord>> toDomain = row => new TrainingRecord
    {
      Id = row.Id,
      Member = new NameIdPair { Id = row.MemberId, Name = row.Member.LastName + ", " + row.Member.FirstName },
      Completed = row.Completed.Value,
      Expiry = row.Expiry,
      Course = new NameIdPair { Id = row.CourseId, Name = row.Course.DisplayName },
      RuleId = row.RuleId,
      Attendance = row.AttendanceId.HasValue ? new NameIdPair { Id = row.Attendance.Id, Name = row.Attendance.Event.Title } : null
    };

    private bool IsTrainingRequired(MemberRow m, TrainingCourseRow course)
    {
      return (course.WacRequired & m.RequiredTrainingFilter) > 0;
    }

    private bool ShouldKeepCourseCurrent(MemberRow m, TrainingCourseRow course)
    {
      return m.IsTrainingRequired(course) && ((course.WacRequired & GetTrainingFilter(m, 0)) > 0);
    }

    private int GetTrainingFilter(MemberRow m, int offset)
    {
      int multiplier = (int)m.WacLevel - 1;
      return 1 << (2 * multiplier + offset);
    }




    public static CompositeTrainingStatus ComputeCompositeTraining(MemberRow m, IEnumerable<ComputedTrainingRecordRow> awards, IEnumerable<TrainingCourseRow> courses, DateTime when)
    {
      Dictionary<Guid, TrainingStatus> expirations = new Dictionary<Guid, TrainingStatus>();
      bool isGood = true;

      foreach (TrainingCourseRow course in courses)
      {
        TrainingStatus status = new TrainingStatus { CourseId = course.Id, CourseName = course.DisplayName, Expires = null, Status = ExpirationFlags.Unknown };

        bool mustHave = m.IsTrainingRequired(course);
        bool keepCurrent = m.ShouldKeepCourseCurrent(course);
        ComputedTrainingRecordRow award = awards.Where(f => f.CourseId == course.Id).FirstOrDefault();
        if (award == null)
        {
          // No record - member has not completed the training
          status.Status = mustHave ? ExpirationFlags.Missing : ExpirationFlags.NotNeeded;
        }
        else if ((course.ValidMonths ?? 0) == 0)
        {
          // A record without an expiration
          status.Status = ExpirationFlags.Complete;
          status.Completed = award.Completed;
        }
        else
        {
          // A record that has an expiration

          status.Expires = award.Expiry;
          status.Completed = award.Completed;
          status.Status = (mustHave && keepCurrent)
                           ? ((award.Expiry < when) ? ExpirationFlags.Expired : ExpirationFlags.NotExpired)
                           : ExpirationFlags.Complete;
        }
        expirations.Add(course.Id, status);

        // Ugh. Now we get to perpetuate a hack for a change in requirements for crime scene with a period when we ignored expired crime scene.
        if (!(course.DisplayName == "Crime Scene" && status.Expires > new DateTime(2007, 03, 01) && when > new DateTime(2010, 6, 3) && when < new DateTime(2010, 11, 12)))
          isGood = isGood && ((status.Status & ExpirationFlags.Okay) == ExpirationFlags.Okay);
      }
      return new CompositeTrainingStatus(expirations, isGood);
    }

    public static IList<TrainingCourseRow> GetCoreCompetencyCourses(IKcsarContext context)
    {
      var courses = new[] {
                "Clues.WE",
                "CPR",
                "Crime.C", "Crime.WE",
                "FirstAid",
                "Fitness.WE",
                "GPS.PE", "GPS.WE",
                "Helo.C", "Helo.WE",
                "Legal.C", "Legal.WE",
                "Management.WE",
                "Nav.PE", "Nav.WE",
                "Radio.PE", "Radio.WE",
                "Rescue.PE", "Rescue.WE",
                "Safety.PE", "Safety.WE",
                "SearchTech.PE", "SearchTech.WE",
                "Survival.PE", "Survival.WE"
            }.Select(f => "Core/" + f).ToList();
      courses.Add("ICS-100");
      courses.Add("ICS-700");

      return (from c in context.TrainingCourses where courses.Contains(c.DisplayName) select c).AsNoTracking().OrderBy(f => f.DisplayName).ToList();
    }

    private IQueryable<ComputedTrainingRecordRow> ComputeForMember(Guid memberId, DateTime when)
    {
      return null;
    }
  }
}
