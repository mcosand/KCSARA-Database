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
        Member m = new Member { FirstName = "Log property" };
        db.Members.Add(m);
        db.SaveChanges();
        memberId = m.Id;
      }
      DateTime checkpoint = DateTime.Now;
      using (var db = new KcsarContext(this.databaseLocation))
      {
        Member m = db.Members.Single(f => f.Id == memberId);
        m.FirstName = "Fixed";
        db.SaveChanges();
        var log = db.GetLog(checkpoint);

        Assert.AreEqual(1, log.Length, "log entries");
        Assert.IsTrue(log[0].Comment.Contains("Log property => Fixed"), "log msg: " + log[0].Comment);
      }
    }
  }
}
