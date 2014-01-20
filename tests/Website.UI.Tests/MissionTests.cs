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
using Kcsar.Database.Model;
using System.Threading;
using Kcsara.Database.Web;

namespace Kcsara.Database.Website.Tests
{
    [TestFixture]
    public class MissionTests
    {
        private Browser browser;

        [TestFixtureSetUp]
        public void Setup()
        {
            //using (var db = new KcsarContext())
            //{
            //    this.member = db.Members.Where(f => f.ContactNumbers.Count > 1
            //        && f.Addresses.Count > 1
            //        && f.MissionRosters.Count > 1
            //        && f.TrainingRosters.Count > 1).First();
            //}
            this.browser = UIAutomation.GetAdminBrowser();
           // this.browser.NavigateToPath("/Members/Detail/" + this.member.Id.ToString());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            UIAutomation.Shutdown();
        }

        [Test]
        public void MissionList()
        {
            this.browser.NavigateToPath("/Missions");

            
        }
    }
}
