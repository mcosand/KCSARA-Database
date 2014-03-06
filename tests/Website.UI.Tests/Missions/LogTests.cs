
namespace Internal.Website.Missions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using Kcsar.Database.Model;
  using NUnit.Framework;
  using OpenQA.Selenium;
  using OpenQA.Selenium.Firefox;
  using OpenQA.Selenium.Support.UI;

  [TestFixture]
  public class LogTests : MissionTestsBase
  {
    [Test]
    public void AddLog()
    {
      using (var db = context.GetDb())
      {
        var member = db.Members.First();

        d.Url = string.Format("{0}/Missions/Roster/{1}", context.Url, this.MissionId);
        d.FindElement(By.ClassName("nav-second")).FindElement(By.LinkText("Log")).Click();

        d.FindElement(By.Id("addlog")).Click();
        var form = d.FindElement(By.Id("logform"));

        // time field should auto-populate with current time
        var time = form.FindElement(By.Id("time"));
        DateTime t = DateTime.Parse(time.GetAttribute("value"));
        Assert.Less(10, Math.Abs((DateTime.Now - t).TotalSeconds), "current time");

        // submit with no time
        time.Clear();
        time.SendKeys("not a time");
        JQueryDialogSubmit(form);
        WaitFor(10, f => f.FindElement(By.Id("time")).GetAttribute("class").Contains("ui-state-error"));
        
        // submit with valid time but no message
        time.Clear();
        time.SendKeys(t.ToString("yyyy-MM-dd HH:mm:ss"));
        JQueryDialogSubmit(form);
        WaitFor(10, f => f.FindElement(By.Id("data")).GetAttribute("class").Contains("ui-state-error"));
        
        // add a message and a data-entry person
        string msg = "This is a log message about the mission";
        form.FindElement(By.Id("data")).SendKeys(msg);

        form.FindElement(By.Id("person")).SendKeys(member.ReverseName);
        PickSuggestedUser(member.Id);

        JQueryDialogSubmit(form);
        WaitFor(10, f => f.FindElement(By.Id("missionlog")).Text.Contains(msg));
      }
    }
  }
}
