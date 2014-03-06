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
  using OpenQA.Selenium.Support.UI;

  public abstract class BrowserFixture
  {
    protected IWebDriver d = null;
    protected string mainWindow = null;
    protected IntegrationContext context = null;

    [TestFixtureSetUp]
    public virtual void FixtureSetup()
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
    public virtual void Teardown()
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
    public virtual void FixtureTeardown()
    {
      d.Quit();
    }

    protected void SwitchToPopup()
    {
      string popupElement = d.WindowHandles.Where(f => f != mainWindow).Single();
      d.SwitchTo().Window(popupElement);
    }


    protected void PickSuggestedUser(Guid userId)
    {
      WebDriverWait wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
      WaitFor(10, f => f.FindElement(By.Id(string.Format("s_{0}", userId))))
        .Click();
    }

    protected T WaitFor<T>(int seconds, Func<IWebDriver, T> act)
    {
      WebDriverWait wait = new WebDriverWait(d, TimeSpan.FromSeconds(seconds));
      return wait.Until(act);
    }

    protected void JQueryDialogSubmit(IWebElement form)
    {
      form.FindElements(By.XPath("..//div[@class='ui-dialog-buttonset']/button")).First().Click();
    }
  }
}
