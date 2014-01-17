/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.Controls.HtmlControls;
using System.Net;
using System.Text.RegularExpressions;

namespace Kcsara.Database.Website.Tests
{
    [TestFixture]
    public class AuthenticationTests
    {
        [TestFixtureTearDown]
        public void TearDown()
        {
            UIAutomation.Shutdown();
        }

        [Test]
        public void BasicFormsLogin()
        {
            Browser b = UIAutomation.GetAdminBrowser();
            Assert.IsNotNull(b.Find.ByCustom<HtmlAnchor>(f => f.HRef.EndsWith("/Account/Logout"), 3000), "Should have link to logout");
        }

        [Test]
        public void BasicAuthLogin()
        {
            string content = DatabaseAutomation.DownloadPageAsAdmin("Missions");
            Console.WriteLine(content);
            Assert.IsFalse(Regex.IsMatch(content, "<input[^>]id=\\\"username\\\"", RegexOptions.IgnoreCase), "Shouldn't contain username login box");
        }
    }
}
