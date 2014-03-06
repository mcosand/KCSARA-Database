namespace Internal.Website.Members
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;
  using NUnit.Framework;
  using OpenQA.Selenium;

  [TestFixture]
  public class MembershipTests : BrowserFixture
  {
    [Test]
    public void AddMembership()
    {
      string name = string.Format("Eustace {0}", Guid.NewGuid());
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
        d.FindElement(By.LinkText("Add/Change Membership")).Click();

        SwitchToPopup();
        d.FindElement(By.Id("Activated")).Clear();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
        Assert.AreEqual(1, fieldErrors.Count, "error count");
        Assert.IsTrue(fieldErrors.All(f => f.Text == "The Activated field is required."), "error text");

        d.FindElement(By.Id("Activated")).SendKeys(DateTime.Today.ToString("yyyy-MM-dd"));
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        using (var db = context.GetDb())
        {
          Assert.AreEqual(1, db.Members.Single(f => f.Id == memberId).Memberships.Count, "membership count");
        }
      }
      finally
      {
        CleanupEntry(name);
      }
    }

    //[Test]
    //public void DeleteMember()
    //{
    //  string name = string.Format("George {0}", Guid.NewGuid());
    //  Guid id;
    //  using (var db = context.GetDb())
    //  {
    //    Member a = new Member { FirstName = name, LastName = "TestUser" };
    //    db.Members.Add(a);
    //    db.SaveChanges();
    //    id=a.Id;
    //  }

    //  try
    //  {
    //    d.Url = string.Format("{0}/members/detail/{1}", context.Url, id);

    //    d.FindElement(By.Id("card"))
    //      .FindElement(By.LinkText("Delete")).Click();
    //    d.FindElement(By.XPath("//input[@type='submit']")).Click();

    //    Assert.AreEqual(context.Url.ToLowerInvariant() + "/members", d.Url.ToLowerInvariant());
    //    Assert.IsFalse(d.FindElements(By.TagName("a")).Any(f => f.Text == name), "is on page");

    //    using (var db = context.GetDb())
    //    {
    //      Assert.IsNull(db.Members.SingleOrDefault(f => f.Id == id), "not in database");
    //    }
    //  }
    //  catch
    //  {
    //    CleanupEntry(name);
    //    throw;
    //  }
    //}

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
