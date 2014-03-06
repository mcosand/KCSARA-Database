namespace Internal.Website.Missions
{
  using System;
  using System.Linq;
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
  }
}
