namespace Internal.Website.Animals
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
  public class AnimalTests : BrowserFixture
  {
    [Test]
    public void AddAnimal()
    {
      string name = string.Format("Spot {0}", Guid.NewGuid());

      try
      {
      d.FindElement(By.LinkText("Animals")).Click();
      d.FindElement(By.LinkText("Create Animal")).Click();

      SwitchToPopup();
      d.FindElement(By.XPath("//input[@type='submit']")).Click();
      var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
      Assert.AreEqual(2, fieldErrors.Count, "error count");
      Assert.IsTrue(fieldErrors.All(f => f.Text == "Required"), "error text");

      d.FindElement(By.Id("Name")).SendKeys(name);
      d.FindElement(By.Id("DemSuffix")).SendKeys("D1");
      d.FindElement(By.XPath("//input[@type='submit']")).Click();
      d.SwitchTo().Window(this.mainWindow);

      Assert.IsNotNull(d.FindElement(By.LinkText(name)), "in updated list");
      }
      finally
      {
        CleanupEntry(name);
      }
    }

    [Test]
    public void DeleteAnimal()
    {
      string name = string.Format("Spot {0}", Guid.NewGuid());

      using (var db = context.GetDb())
      {
        Animal a = new Animal { Name = name, DemSuffix = "D1" };
        db.Animals.Add(a);
        db.SaveChanges();
      }

      try
      {
        d.FindElement(By.LinkText("Animals")).Click();
        d.FindElement(By.LinkText(name))
          .FindElement(By.XPath("../.."))
          .FindElement(By.LinkText("Delete"))
          .Click();

        SwitchToPopup();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        d.SwitchTo().Window(mainWindow);

        Assert.IsFalse(d.FindElements(By.TagName("a")).Any(f => f.Text == name), "is on page");
      }
      catch
      {
        CleanupEntry(name);
        throw;
      }
    }

    [Test]
    public void SetAnimalOwner()
    {
      string name = string.Format("Fred {0}", Guid.NewGuid());

      Guid id;

      string owner;
      Guid userId;
      using (var db = context.GetDb())
      {
        Animal a = new Animal { Name = name, DemSuffix = "D1" };
        db.Animals.Add(a);
        db.SaveChanges();
        id = a.Id;

        Member m = db.Members.First();
        owner = m.ReverseName;
        userId = m.Id;
      }

      try
      {
        d.Url = context.Url + "/Animals/Detail/" + id.ToString();
        d.FindElement(By.LinkText("Add New Owner")).Click();
        SwitchToPopup();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();
        var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
        Assert.AreEqual(1, fieldErrors.Count, "error count");
        Assert.IsTrue(fieldErrors.All(f => f.Text.StartsWith("Required")), "error text");

        d.FindElement(By.Id("name_a")).SendKeys(owner);
        PickSuggestedUser(userId);

        d.FindElement(By.Id("IsPrimary")).Click();
        
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        d.SwitchTo().Window(mainWindow);
        Assert.IsNotNull(d.FindElement(By.LinkText(owner)), "link to owner");
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
        var a = db.Animals.SingleOrDefault(f => f.Name == name);
        if (a != null)
        {
          db.Animals.Remove(a);
          db.SaveChanges();
        }
      }
    }
  }
}
