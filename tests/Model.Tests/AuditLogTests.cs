/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Model
{
  using System;
  using System.Linq;
  using System.Threading;
  using Kcsar.Database.Model;
  using NUnit.Framework;

  [TestFixture]
  public class AuditLogTests
  {
    string databaseLocation = string.Empty;

    [TestFixtureSetUp]
    public void Setup()
    {
      this.databaseLocation = DatabaseTestHelpers.CreateTestDatabase();
    }

    [TestFixtureTearDown]
    public void Teardown()
    {
      DatabaseTestHelpers.DeleteDatabase(this.databaseLocation);
    }

    [Test]
    public void LogPropertyChanged()
    {
      Guid memberId;
      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = new Member {
          FirstName = "Log property",
          Gender = Gender.Female,
          BirthDate = new DateTime(2000, 12, 19)
        };
        db.Members.Add(m);
        db.SaveChanges();
        memberId = m.Id;
      }

      DateTime checkpoint = GetCheckpoint();
      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = db.Members.Single(f => f.Id == memberId);
        m.FirstName = "Fixed";
        m.BirthDate = new DateTime(1990, 3, 5);
        m.Gender = Gender.Male;
        db.SaveChanges();
        var log = db.GetLog(checkpoint);

        Assert.AreEqual(1, log.Length, "log entries");
        Console.WriteLine(log[0].Comment);
        Assert.IsTrue(log[0].Comment.Contains("FirstName: Log property => Fixed"), "log msg: " + log[0].Comment);
        Assert.IsTrue(log[0].Comment.Contains("Gender: Female => Male"), "log msg gender: " + log[0].Comment);
        Assert.IsTrue(log[0].Comment.Contains("BirthDate: 12/19/2000 => 3/5/1990"), "log msg date: " + log[0].Comment);
        Assert.AreEqual("Modified", log[0].Action, "action: " + log[0].Action);
      }
    }

    [Test]
    public void LogObjectDeleted()
    {
      Guid memberId;
      string reportHtml;
      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = new Member { FirstName = "RemoveMe" };
        db.Members.Add(m);
        db.SaveChanges();
        memberId = m.Id;
        reportHtml = m.GetReportHtml();
      }

      DateTime checkpoint = GetCheckpoint();

      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = db.Members.Single(f => f.Id == memberId);
        db.Members.Remove(m);
        db.SaveChanges();
        var log = db.GetLog(checkpoint);

        Assert.AreEqual(1, log.Length, "log entries");
        Assert.AreEqual("Deleted", log[0].Action, "action: " + log[0].Action);
        Assert.AreEqual(reportHtml, log[0].Comment, "log msg: " + log[0].Comment);
      }
    }


    [Test]
    public void LogObjectCreated()
    {
      Guid memberId;
      DateTime checkpoint;
      checkpoint = GetCheckpoint();

      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = new Member { FirstName = "NewUser" };
        db.Members.Add(m);
        db.SaveChanges();
        memberId = m.Id;
      }

      using (var db = new KcsarContext(this.databaseLocation))
      {
        var member = db.Members.Single(f => f.Id == memberId);
        var logs = db.GetLog(checkpoint);
        Assert.AreEqual(1, logs.Length, "log entries");
        Assert.AreEqual(member.GetReportHtml(), logs[0].Comment, "log msg: " + logs[0].Comment);
        Assert.AreEqual("Added", logs[0].Action, "action: " + logs[0].Action);
      }
    }

    [Test]
    public void LogPrincipalChanged()
    {
      Member first;
      Member second;
      PersonAddress address;
      using (var db = new KcsarContext(this.databaseLocation))
      {
        first = new Member { FirstName = "First" };
        db.Members.Add(first);
        second = new Member { FirstName = "Second" };
        db.Members.Add(second);
        address = new PersonAddress { Person = first, Street = "123", City = "Any", State = "WA", Zip = "98765" };
        first.Addresses.Add(address);

        db.SaveChanges();
      }

      var checkpoint = GetCheckpoint();
      using (var db = new KcsarContext(this.databaseLocation))
      {
        address = db.Members.Where(f => f.Id == first.Id).SelectMany(f => f.Addresses).Single();
        address.Person = db.Members.Single(f => f.Id == second.Id);
        db.SaveChanges();
      }

      using (var db = new KcsarContext(this.databaseLocation))
      {
        var logs = db.GetLog(checkpoint);
        Assert.AreEqual(1, logs.Length, "log count");
        Assert.AreEqual(string.Format("{0}<br/>{1} => {2}",
          address, first, second), logs[0].Comment);
        Assert.AreEqual("Modified", logs[0].Action);
      }       
    }

    // Wait long enough to pass the resolution of the sql time check.
    private static DateTime GetCheckpoint()
    {
      Thread.Sleep(200);
      DateTime checkpoint = DateTime.Now;
      Thread.Sleep(200);
      return checkpoint;
    }
  }
}
