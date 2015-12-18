/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Model
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;
  using NUnit.Framework;

  [TestFixture]
  public class AnimalMissionTests
  {
    [Test]
    public void ReportFormat()
    {
      string missionName = "Test Mission";
      string animalName = "Animal Bob";
      DateTime timein = DateTime.Now;
      DateTime? timeout = null;

      var am = new AnimalMission()
      {
        MissionRoster = new MissionRoster_Old {
          Mission = new Mission_Old { Title = missionName },
          TimeIn = timein,
          TimeOut = timeout
        },
        Animal = new Animal { Name = animalName }
      };

      Assert.AreEqual(string.Format(AnimalMission.ReportFormat, am.MissionRoster.Mission.Title, am.Animal.Name, am.MissionRoster.TimeIn, am.MissionRoster.TimeOut), am.GetReportHtml());
    }

    [Test]
    public void Validate()
    {
      var am = new AnimalMission();
      Assert.AreEqual(0, am.Validate(null).Count());
    }
  }
}
