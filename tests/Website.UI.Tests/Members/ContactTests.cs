namespace Internal.Website.Members
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;
  using NUnit.Framework;
  using OpenQA.Selenium;
  using OpenQA.Selenium.Support.UI;

  [TestFixture]
  public class ContactTests : BrowserFixture
  {
    [Test]
    public void AddContact()
    {
      string name = string.Format("Clarence {0}", Guid.NewGuid());
      Guid memberId;

      using (var db = context.GetDb())
      {
        Member a = new Member { FirstName = name, LastName = "TestUser" };
        db.Members.Add(a);
        db.SaveChanges();
        memberId = a.Id;
      }
      
      try
      {
        d.Url = string.Format("{0}/members/detail/{1}", context.Url, memberId);
        d.FindElement(By.Id("addContact")).Click();

        var form = d.FindElement(By.Id("contactform"));
        new SelectElement(d.FindElement(By.Id("contactType"))).SelectByText("phone");
        new SelectElement(d.FindElement(By.Id("contactSubtype"))).SelectByText("work");
        form.FindElements(By.XPath("..//div[@class='ui-dialog-buttonset']/button")).First().Click();
        var tips = form.FindElement(By.ClassName("validateTips"));
        Assert.AreEqual(1, tips.FindElements(By.TagName("span")).Count);
        Assert.AreEqual("Value: Required", tips.Text, "tip text");
        Assert.IsTrue(tips.Displayed, "err tips displayed");

        form.FindElement(By.Id("contactValue")).SendKeys("123-456-7890");
        form.FindElements(By.XPath("..//div[@class='ui-dialog-buttonset']/button")).First().Click();

        WaitFor(10, f => (f.FindElement(By.Id("contactform")).Displayed == false));

        Assert.IsTrue(d.FindElement(By.Id("contacts_table")).Text.Contains("123-456-7890"), "table has number");

        using (var db = context.GetDb())
        {
          Assert.AreEqual(1, db.Members.Single(f => f.Id == memberId).ContactNumbers.Count, "contact count");
        }
      }
      finally
      {
        CleanupEntry(name);
      }
    }

    [Test]
    public void PromoteContact()
    {
      string name = string.Format("Clareece {0}", Guid.NewGuid());
      Guid memberId;

      using (var db = context.GetDb())
      {
        Member a = new Member { FirstName = name, LastName = "TestUser" };
        db.Members.Add(a);
        a.ContactNumbers.Add(new PersonContact { Type = "email", Value = "a@example.com", Priority = 0 });
        a.ContactNumbers.Add(new PersonContact { Type = "email", Value = "b@example.com", Priority = 1 });
        db.SaveChanges();
        memberId = a.Id;
      }

      try
      {
        d.Url = string.Format("{0}/members/detail/{1}", context.Url, memberId);
        var table = d.FindElement(By.Id("contacts_table"));
        var link = table.FindElement(By.LinkText("Promote"));
        Assert.IsTrue(link.FindElement(By.XPath("../..")).Text.Contains("b@example.com"), "promote link on correct entry");
        link.Click();

        WaitFor(10, f => table.FindElement(By.LinkText("Promote"))
          .FindElement(By.XPath("../..")).Text.Contains("a@example.com"));
      }
      finally
      {
        CleanupEntry(name);
      }
    }
    private void CleanupEntry(string name)
    {
      using (var db = context.GetDb())
      {
        var a = db.Members.SingleOrDefault(f => f.FirstName == name);
        if (a != null)
        {
          db.Members.Remove(a);
          db.SaveChanges();
        }
      }
    }
  }
}
