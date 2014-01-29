using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsara.Database.Web.api.Models;
using NUnit.Framework;
using M = Kcsar.Database.Model;

namespace Internal.Website.Api.Models
{
  [TestFixture]
  public class MissionResponseStatusTests
  {
    [Test]
    public void ShouldStage_NoDataRow()
    {
      var m = GetMission();
      m.ResponseStatus = null;
      Assert.IsFalse(Convert(m).ShouldStage);
    }

    [Test]
    public void ShouldStage_ColumnNull()
    {
      var m = GetMission();
      m.ResponseStatus.StopStaging = null;
      Assert.IsTrue(Convert(m).ShouldStage);
    }

    [Test]
    public void ShouldStage_FutureDate()
    {
      var m = GetMission();
      m.ResponseStatus.StopStaging = DateTime.Now.AddHours(1);
      Assert.IsTrue(Convert(m).ShouldStage);
    }

    [Test]
    public void ShouldStage_PastDate()
    {
      var m = GetMission();
      m.ResponseStatus.StopStaging = DateTime.Now.AddHours(-1);
      Assert.IsFalse(Convert(m).ShouldStage);
    }

    [TestCase(null)]
    [TestCase(-3)]
    [TestCase(3)]
    [TestCase(15)]
    public void ShouldCall_FutureDate(int? stopStagingHours)
    {
      // if there is an ops period in the future you can always call for it
      var m = GetMission();
      m.ResponseStatus.CallForPeriod = DateTime.Now.AddHours(10);
      m.ResponseStatus.StopStaging = stopStagingHours.HasValue ? DateTime.Now.AddHours(stopStagingHours.Value) : (DateTime?)null;
      Assert.IsTrue(Convert(m).ShouldCall);
    }

    // No one has said "enough", assume they want more
    [TestCase(null, true)]
    // Someone has said enough, but that was for the period before this most recent one
    [TestCase(-13, true)]
    // Someone has said enough (in the past) since this period started. No more needed.
    [TestCase(-7, false)]
    // Someone has said enough (in the future) since this period started. Should call until stop staging time.
    [TestCase(3, true)]
    public void ShouldCall_PastDate(int? stopStagingHours, bool expected)
    {
      var m = GetMission();
      m.ResponseStatus.CallForPeriod = DateTime.Now.AddHours(-10);
      m.ResponseStatus.StopStaging = stopStagingHours.HasValue ? DateTime.Now.AddHours(stopStagingHours.Value) : (DateTime?)null;
      Assert.AreEqual(expected, Convert(m).ShouldCall);
    }


    private M.Mission GetMission()
    {
      return new M.Mission
      {
        Title = "Test",
        ResponseStatus = new M.MissionResponseStatus 
        {
          StopStaging = null,
          CallForPeriod = DateTime.Now.AddHours(-1)
        }
      };
    }

    private MissionResponseStatus Convert(M.Mission mission)
    {
      var result = MissionResponseStatus.FromData(mission);
      return result;      
    }
  }
}
