namespace Internal.Website.Training
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
  public class CourseTests : BrowserFixture
  {
    [Test]
    public void EditCourse()
    {
      string name = string.Format("Course {0}", Guid.NewGuid());
      Guid id;
      using (var db = context.GetDb())
      {
        var c = new TrainingCourse
        {
          FullName = name,
          DisplayName = "Test Course",
          Unit = db.Units.First(),
          Categories = "other"
        };
        db.TrainingCourses.Add(c);
        db.SaveChanges();
        id = c.Id;
      }

      try
      {
        d.Url = string.Format("{0}/training/current/{1}", context.Url, id);
 
        d.FindElement(By.LinkText("Properties...")).Click();

        SwitchToPopup();
        d.FindElement(By.Id("DisplayName")).Clear();
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        var fieldErrors = d.FindElements(By.ClassName("field-validation-error"));
        Assert.AreEqual(1, fieldErrors.Count, "error count");
        Assert.IsTrue(fieldErrors.All(f => f.Text == "*"), "error text");

        d.FindElement(By.Id("DisplayName")).SendKeys("New name");
        d.FindElement(By.XPath("//input[@type='submit']")).Click();

        d.SwitchTo().Window(mainWindow);
        Assert.IsTrue(d.PageSource.Contains("New name"), "has updated name");
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
        var a = db.TrainingCourses.SingleOrDefault(f => f.FullName == name);
        if (a != null)
        {
          db.TrainingCourses.Remove(a);
          db.SaveChanges();
        }
      }
    }
  }
}
