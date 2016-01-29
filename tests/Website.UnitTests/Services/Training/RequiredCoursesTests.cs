using System;
using System.Collections.Generic;
using DB = Kcsar.Database.Model;
using Kcsara.Database.Web.Models;
using Kcsara.Database.Web.Services;
using NSubstitute;
using Kcsara.Database.Web.Models.Training;
using Xunit;

namespace Internal.Website.Services.Training
{
  public class RequiredCoursesTests
  {
    [Fact]
    public void CompletedParentGivesChildren()
    {
      var c = Context.Build();
      var expires = DateTime.Now.AddYears(1);
      var completed = new Dictionary<Guid, DB.ComputedTrainingAwardRow>
      {
        { c.Course.CourseId, new DB.ComputedTrainingAwardRow { CourseId = c.Course.CourseId, Completed = DateTime.Now.AddDays(-1), Expiry = expires, MemberId = c.MemberId } }
      };

      // TEST
      var result = c.Compute(completed);

      // VERIFY
      var resultCourse = (CompositeTrainingStatus)result[0].Courses[0];
      Assert.True(Equals(expires, resultCourse.Expires), "expires");
      Assert.True(Equals(ExpirationFlags.NotExpired, resultCourse.Status), "status");
      Assert.True(Equals(c.Course.Parts.Count, resultCourse.Parts.Count), "parts count");
      for (var i=0;i< resultCourse.Parts.Count; i++)
      {
        Assert.True(Equals(expires, resultCourse.Parts[i].Expires), "child " + i + " expires");
        Assert.True(Equals(ExpirationFlags.NotExpired, resultCourse.Parts[i].Status), "child " + i + " status");
      }
    }

    [Fact]
    public void NoAwardsForCompositeRequirement()
    {
      var c = Context.Build();
      var completed = new Dictionary<Guid, DB.ComputedTrainingAwardRow>();

      var result = c.Compute(completed);

      var resultCourse = (CompositeTrainingStatus)result[0].Courses[0];
      XAssert.Equals(ExpirationFlags.Missing, resultCourse.Status, "status");
      Assert.False(resultCourse.Expires.HasValue, "expires");
      Assert.False(resultCourse.Completed.HasValue, "completed");
      XAssert.Equals(c.Course.Parts.Count, resultCourse.Parts.Count, "parts count");
      for (var i=0;i<resultCourse.Parts.Count; i++)
      {
        XAssert.Equals(ExpirationFlags.Missing, resultCourse.Parts[i].Status, "child " + i + " status");
        Assert.False(resultCourse.Parts[i].Expires.HasValue, "expires");
        Assert.False(resultCourse.Parts[i].Completed.HasValue, "completed");
      }
    }

    [Fact]
    public void ParentWithUpdatedChild()
    {
      var c = Context.Build();
      var expires = DateTime.Now.AddYears(1);
      var completed = new Dictionary<Guid, DB.ComputedTrainingAwardRow>
      {
        { c.Course.CourseId, new DB.ComputedTrainingAwardRow { CourseId = c.Course.CourseId, Completed = DateTime.Now.AddDays(-5), Expiry = expires, MemberId = c.MemberId } },
        { c.Course.Parts[1].Id, new DB.ComputedTrainingAwardRow { CourseId = c.Course.Parts[1].Id, Completed = DateTime.Today.AddDays(-2), Expiry = expires.AddDays(1), MemberId = c.MemberId } }
      };

      // TEST
      var result = c.Compute(completed);

      // VERIFY
      var resultCourse = (CompositeTrainingStatus)result[0].Courses[0];
      XAssert.Equals(expires, resultCourse.Expires, "expires");
      XAssert.Equals(ExpirationFlags.NotExpired, resultCourse.Status, "status");
      XAssert.Equals(c.Course.Parts.Count, resultCourse.Parts.Count, "parts count");
      for (var i = 0; i < resultCourse.Parts.Count; i++)
      {
        if (i == 1)
        {
          XAssert.Equals(completed[resultCourse.Parts[i].Course.Id].Expiry, resultCourse.Parts[i].Expires, "completed child expires");
        }
        else
        {
          XAssert.Equals(expires, resultCourse.Parts[i].Expires, "child " + i + " expires");
        }
        XAssert.Equals(ExpirationFlags.NotExpired, resultCourse.Parts[i].Status, "child " + i + " status");
      }
    }

    class Context
    {
      public Guid MemberId;
      public Dictionary<Guid, string> Courses;
      public TrainingService.RequiredScope CountyScope;
      public TrainingService.RequiredCourse Course;
      public TrainingService Service;

      public List<ScopedTrainingStatus> Compute(Dictionary<Guid, DB.ComputedTrainingAwardRow> completed)
      {
        return Service.ComputedRequiredTrainingStatus(MemberId, DateTime.Now, Courses, completed);
      }

      public static Context Build()
      {
        var result = new Context();

        result.Course = new TrainingService.RequiredCourse
        {
          CourseId = Guid.NewGuid(),
          MinWacLevel = DB.WacLevel.Support,
          Parts = new List<Kcsara.Database.Web.Models.NameIdPair>
        {
          new NameIdPair { Id = Guid.NewGuid(), Name = "Part A" },
          new NameIdPair { Id = Guid.NewGuid(), Name = "Part B" },
          new NameIdPair { Id = Guid.NewGuid(), Name = "Part C" }
        }
        };
        result.CountyScope = new TrainingService.RequiredScope
        {
          Name = "County Scope",
          Courses = new List<TrainingService.RequiredCourse> { result.Course }
        };

        result.MemberId = Guid.NewGuid();
        result.Courses = new Dictionary<Guid, string>
        {
          { result.Course.CourseId, "Parent Course" },
          { result.Course.Parts[0].Id, "Course A" },
          { result.Course.Parts[1].Id, "Course B" },
          { result.Course.Parts[2].Id, "Course C" }
        };

        result.Service = new TrainingService(() => Substitute.For<DB.IKcsarContext>(), new ConsoleLogger(), new List<TrainingService.RequiredScope> { result.CountyScope });

        return result;
      }
    }
  }
}
