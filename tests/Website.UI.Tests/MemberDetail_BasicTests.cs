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
    public class MemberDetail_BasicTests
    {
        private Browser browser;
        private Member member;

        [TestFixtureSetUp]
        public void Setup()
        {
            using (var db = new KcsarContext())
            {
                this.member = db.Members.Where(f => f.ContactNumbers.Count > 1
                    && f.Addresses.Count > 1
                    && f.MissionRosters.Count > 1
                    && f.TrainingRosters.Count > 1).First();
            }
            this.browser = UIAutomation.GetAdminBrowser();
            this.browser.NavigateToPath("/Members/Detail/" + this.member.Id.ToString());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            UIAutomation.Shutdown();
        }

        [Test]
        public void Contacts()
        {
            var table = this.browser.Find.ById<HtmlTable>("contacts_table");
            Assert.IsNotNull(table, "Need table of contact information");

            for (int i = 0; i < 10; i++)
            {
                if (!table.InnerText.Contains(Strings.Loading))
                    break;
                Thread.Sleep(1000);
                this.browser.RefreshDomTree();
                table.Refresh();
            }

            using (var db = new KcsarContext())
            {
                Assert.AreEqual(db.PersonContact.Where(f => f.Person.Id == this.member.Id).Count(), table.Rows.Count, "Table rows should be equal");
            }
            Console.WriteLine("Found {0} contacts", table.Rows.Count);
        }

        [Test]
        public void Addresses()
        {
            var table = this.browser.Find.ById<HtmlTable>("address_table");
            Assert.IsNotNull(table, "Need table of addresses");

            using (var db = new KcsarContext())
            {
                var addresses = db.PersonAddress.Where(f => f.Person.Id == this.member.Id).ToList();

                Assert.AreEqual(addresses.Count, table.Rows.Count, "Table rows should be equal");

                addresses.ForEach(f =>
                    {
                        Assert.IsTrue(table.Rows.Any(g => g.Cells[1].TextContent == f.SimpleText), "Find address " + f.SimpleText);
                    });
            }
            Console.WriteLine("Found {0} addresses", table.Rows.Count);                        
        }

        [Test]
        public void PersonalInformation()
        {
            var card = this.browser.Find.ById<HtmlDiv>("card");
            Assert.IsTrue(card.InnerText.Contains(this.member.FullName));
        }
    }
}
