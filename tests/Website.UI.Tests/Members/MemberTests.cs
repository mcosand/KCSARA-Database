namespace Internal.Website.Members
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
  public class MemberTests : BrowserFixture
  {
    [Test]
    public void AddMember()
    {
      string name = string.Format("George {0}", Guid.NewGuid());

      try
      {
        d.FindElement(By.LinkText("Members")).Click();
        d.FindElement(By.LinkText("New ...")).Click();

        SwitchToPopup();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
        Assert.AreEqual(1, fieldErrors.Count, "error count");
        Assert.IsTrue(fieldErrors.All(f => f.Text == "Required"), "error text");

        d.FindElement(By.Id("FirstName")).SendKeys(name);
        d.FindElement(By.Id("LastName")).SendKeys("TestUser");
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        Assert.IsTrue(d.FindElement(By.Id("card")).Text
          .Contains(name + " TestUser"));
        d.Close();
      }
      finally
      {
        CleanupEntry(name);
      }
    }

    [Test]
    public void DeleteMember()
    {
      string name = string.Format("George {0}", Guid.NewGuid());
      Guid id;
      using (var db = context.GetDb())
      {
        Member a = new Member { FirstName = name, LastName = "TestUser" };
        db.Members.Add(a);
        db.SaveChanges();
        id=a.Id;
      }

      try
      {
        d.Url = string.Format("{0}/members/detail/{1}", context.Url, id);

        d.FindElement(By.Id("card"))
          .FindElement(By.LinkText("Delete")).Click();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        Assert.AreEqual(context.Url.ToLowerInvariant() + "/members", d.Url.ToLowerInvariant());
        Assert.IsFalse(d.FindElements(By.TagName("a")).Any(f => f.Text == name), "is on page");

        using (var db = context.GetDb())
        {
          Assert.IsNull(db.Members.SingleOrDefault(f => f.Id == id), "not in database");
        }
      }
      catch
      {
        CleanupEntry(name);
        throw;
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
