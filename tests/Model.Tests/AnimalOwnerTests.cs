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
  public class AnimalOwnerTests
  {
    [Test]
    public void ReportFormat()
    {
      string animalName = "Animal Bob";
      DateTime starting = DateTime.Now;
      DateTime? ending = null;
      var owner = new MemberRow { FirstName = "Fred", LastName = "Rider" };
      var ao = new AnimalOwner {
        Owner = owner,
        Animal = new Animal { Name = animalName },
        Starting = starting,
        Ending = ending
      };

      var html = ao.GetReportHtml();
      Assert.IsTrue(html.Contains(animalName), "animal name");
      Assert.IsTrue(html.Contains(owner.FullName), "owner name");
      Assert.IsTrue(html.Contains(starting.ToString()), "starting");
    }

    [Test]
    public void Validate()
    {
      var am = new AnimalMission();
      Assert.AreEqual(0, am.Validate(null).Count());
    }
  }
}
