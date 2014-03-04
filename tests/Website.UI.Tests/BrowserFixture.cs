namespace Internal.Website
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using NUnit.Framework;
  using OpenQA.Selenium;
  using OpenQA.Selenium.Firefox;

  public abstract class BrowserFixture
  {
    protected IWebDriver d = null;
    protected string mainWindow = null;
    protected IntegrationContext context = null;

    [TestFixtureSetUp]
    public void Setup()
    {
      this.context = IntegrationContext.Load();
      this.d = new FirefoxDriver();
      d.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));

      d.Url = context.Url + "/account/login";
      d.FindElement(By.Id("username")).SendKeys(context.AdminUser);
      d.FindElement(By.Id("password")).SendKeys(context.KnownUsers[context.AdminUser]);
      d.FindElement(By.Id("login")).Click();
      mainWindow = d.CurrentWindowHandle;
    }

    [TearDown]
    public void Teardown()
    {
      var toClose = d.WindowHandles.Where(f => f != mainWindow);
      foreach (var w in toClose)
      {
        d.SwitchTo().Window(w);
        d.Close();
      }
      d.SwitchTo().Window(mainWindow);
      d.Url = "http://localhost:4944";
    }

    [TestFixtureTearDown]
    public void FixtureTeardown()
    {
      d.Quit();
    }

    protected void SwitchToPopup()
    {
      string popupElement = d.WindowHandles.Where(f => f != mainWindow).Single();
      d.SwitchTo().Window(popupElement);
    }
  }
}
