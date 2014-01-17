/*
 * Copyright 2012-2014 Matthew Cosand
 */
using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Website.Tests
{
    public static class UIAutomation
    {
        private static Manager manager = null;
        private static Browser browser = null;

        public static BrowserType BrowserType { get; set; }

        static UIAutomation()
        {
            UIAutomation.BrowserType = BrowserType.FireFox;
        }

        public static void Shutdown()
        {
            if (UIAutomation.manager != null)
            {
                foreach (var browser in UIAutomation.manager.Browsers)
                {
                    browser.Close();
                }
                UIAutomation.manager.Dispose();
                UIAutomation.manager = null;
            }
        }

        public static void CloseBrowser()
        {
            if (UIAutomation.browser != null)
            {
                UIAutomation.browser.Close();
                UIAutomation.browser = null;
            }
        }

        public static Browser GetAdminBrowser()
        {
            Browser b = GetBrowserAtUrl(new Uri(DatabaseAutomation.GetDatabaseUrl(), "Account/Login"));
            var cred = DatabaseAutomation.GetAdminCredential();
            b.Find.ById<HtmlInputText>("username").Text = cred.UserName;
            b.Find.ById<HtmlInputPassword>("password").Text = cred.Password;
            b.Find.ById<HtmlInputSubmit>("login").Click();
            return b;
        }

        public static Browser GetBrowserAtUrl(Uri url)
        {
            if (UIAutomation.browser != null && UIAutomation.browser.BrowserType != UIAutomation.BrowserType)
            {
                UIAutomation.CloseBrowser();
            }

            if (UIAutomation.browser == null)
            {
                StartTelerikManager();
                UIAutomation.manager.LaunchNewBrowser(UIAutomation.BrowserType);
                UIAutomation.browser = UIAutomation.manager.ActiveBrowser;
                UIAutomation.browser.Closing += new EventHandler(browser_Closing);
                UIAutomation.browser.WaitUntilReady();
            }
            UIAutomation.browser.NavigateTo(url);
            UIAutomation.browser.WaitUntilReady();
            return UIAutomation.browser;
        }

        private static void StartTelerikManager()
        {
            if (UIAutomation.manager == null)
            {
                Settings settings = new Settings();
                settings.Web.RecycleBrowser = true;
                settings.Web.KillBrowserProcessOnClose = false;

                UIAutomation.manager = new Manager(settings);

                UIAutomation.manager.Start();
            }
        }

        static void browser_Closing(object sender, EventArgs e)
        {
            if ((Browser)sender == UIAutomation.browser)
            {
                UIAutomation.browser.Closing -= new EventHandler(browser_Closing);
                UIAutomation.browser = null;
            }
        }
    }


}
