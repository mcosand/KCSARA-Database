namespace Internal.Website.Missions
{
  using System;
  using System.Data.Entity;
  using System.Linq;
  using Kcsar.Database.Data;
  using Kcsar.Database.Data.Events;
  using Kcsar.Database.Model;
  using NUnit.Framework;
  using OpenQA.Selenium;

  [TestFixture]
  public class RosterTests : MissionTestsBase
  {
    [Test]
    public void NoTimeIn()
    {
      using (var db = context.GetDb())
      {
        var member = db.Members.First();

        var url = string.Format("{0}/Missions/EditRoster/{1}", context.Url, this.MissionId);
        d.Url = url;

        var namebox = d.FindElements(By.XPath("//input[starts-with(@id, 'name_')]")).First();
        Guid rowId = Guid.Parse(namebox.GetAttribute("id").Substring(5));
        namebox.SendKeys(member.ReverseName);
        PickSuggestedUser(member.Id);
        d.FindElement(By.XPath("//input[@value = 'Finish Roster']")).Click();
        Assert.AreEqual("Please correct errors", d.FindElement(By.Id("msg_error")).Text, "err banner");
        Assert.AreEqual(url.ToLowerInvariant(), d.Url.ToLowerInvariant(), "same page");
      }
    }

    [Test]
    public void AddEntry()
    {
      MemberRow member;
      MissionRow mission;
      int rowCount;
      using (var db = context.GetDb())
      {
        member = db.Members.AsNoTracking().First();
        mission = db.Events.AsNoTracking().OfType<MissionRow>().Single(f => f.Id == MissionId);
        rowCount = mission.Roster.Count;
      }
      var start = mission.StartTime.ToString("yyMMdd");

      try
      {
        var url = string.Format("{0}/Missions/EditRoster/{1}", context.Url, this.MissionId);
        d.Url = url;

        var namebox = d.FindElements(By.XPath("//input[starts-with(@id, 'name_')]")).First();
        Guid rowId = Guid.Parse(namebox.GetAttribute("id").Substring(5));
        namebox.SendKeys(member.ReverseName);
        PickSuggestedUser(member.Id);

        d.FindElement(By.Id(string.Format("in{0}_{1}", start, rowId))).SendKeys("800");
        d.FindElement(By.Id(string.Format("out{0}_{1}", start, rowId))).SendKeys("1400");


        d.FindElement(By.XPath("//input[@value = 'Finish Roster']")).Click();
        using (var db = context.GetDb())
        {
          Assert.AreEqual(rowCount + 1, db.Events.Single(f => f.Id == MissionId).Roster.Count, "row count");
          Assert.IsTrue(d.FindElement(By.Id("roster")).Text.Contains(member.ReverseName), "roster updated");
        }
      }
      finally
      {
        using (var db = context.GetDb())
        {
          foreach (var entry in db.Members.Single(f => f.Id == member.Id).Participation.Select(f => f.Event).Where(f => f.Id == MissionId).ToArray())
          {
            db.Events.Remove(entry);
          }
          db.SaveChanges();
        }
      }
    }
  }
}
