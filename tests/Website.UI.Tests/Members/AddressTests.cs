namespace Internal.Website.Animals
{
  using System;
  using System.Linq;
  using Kcsar.Database.Model;
  using NUnit.Framework;
  using OpenQA.Selenium;

  [TestFixture]
  public class AddressTests : BrowserFixture
  {
    [Test]
    public void AddAddress()
    {
      string name = string.Format("Angus {0}", Guid.NewGuid());
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
        d.FindElement(By.LinkText("Add Address")).Click();

        SwitchToPopup();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
        Assert.AreEqual(3, fieldErrors.Count, "error count");
        Assert.IsTrue(fieldErrors.All(f => f.Text.EndsWith("field is required.")), "error text");

        d.FindElement(By.Id("Street")).SendKeys("1234 Main St");
        d.FindElement(By.Id("City")).SendKeys("Anytown");
        d.FindElement(By.Id("Zip")).SendKeys("12345");
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        // popup closes

        d.SwitchTo().Window(mainWindow);
        Assert.IsTrue(d.FindElement(By.Id("address_table"))
          .Text.Contains("1234 Main St"), "detail page refreshed");

        using (var db = context.GetDb())
        {
          Assert.AreEqual(1, db.Members.Single(f => f.Id == memberId).Addresses.Count, "address count");
        }
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
