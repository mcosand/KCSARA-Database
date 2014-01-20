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
    public class MemberDetail_EditTrainingTests
    {
        private Browser browser;
        private Guid memberId;

        [TestFixtureSetUp]
        public void Setup()
        {
            using (var db = new KcsarContext())
            {
                Guid empty = Guid.Empty;
                this.memberId = db.Members.Where(f => f.TrainingAwards.Count  > 1 && f.Id != empty).First().Id;
            }
            try
            {
                this.browser = UIAutomation.GetAdminBrowser();
                this.browser.NavigateToPath("/Members/Detail/" + this.memberId.ToString());
            }
            catch
            {
                UIAutomation.Shutdown();
                throw;
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            UIAutomation.Shutdown();
        }

        [Test]
        public void DuplicateRecordIsCaught()
        {
            TrainingAward existing;
            using (var db = new KcsarContext())
            {
                existing = db.TrainingAward.Where(f => f.Member.Id == memberId).First();
                var find = this.browser.Find;

                var addAward = find.ById<HtmlButton>("addrecord", 3000);
                Assert.IsNotNull(addAward, "Can't find add record button");
                addAward.Click();

                var courseDropdown = find.ById<HtmlSelect>("recordcourse", 2000);
                courseDropdown.SelectByValue(existing.Course.Id.ToString());

                var completedBox = find.ById<HtmlInputText>("recordcompleted");
                completedBox.Text = existing.Completed.ToString("yyyy-MM-dd HH:mm:ss");

                var commentBox = find.ById<HtmlTextArea>("recordcomments");
                commentBox.Text = "Test Comment";

                
                var saveButton = find.ById<HtmlDiv>("editRecordDialog").Parent<HtmlDiv>().Find.ByCustom<HtmlButton>(f => f.InnerText == "Save");
                Assert.IsNotNull(saveButton);
                saveButton.Click();
                
                System.Threading.Thread.Sleep(3000);

                
                var msgP = find.ById<HtmlContainerControl>("editRecordError");
                Assert.IsNotNullOrEmpty(msgP.InnerText);
                Assert.IsTrue(msgP.IsVisible());
            }
        }

        [Test]
        public void UnsubstantiatedRecordIsCaught()
        {
            TrainingAward existing;
            using (var db = new KcsarContext())
            {
                existing = db.TrainingAward.Where(f => f.Member.Id == memberId).First();
                var find = this.browser.Find;

                var addAward = find.ById<HtmlButton>("addrecord", 3000);
                Assert.IsNotNull(addAward, "Can't find add record button");
                addAward.Click();

                var courseDropdown = find.ById<HtmlSelect>("recordcourse", 2000);
                courseDropdown.SelectByValue(existing.Course.Id.ToString());

                DateTime date = existing.Completed;
                do
                {
                    date = date.AddDays(-1);
                } while (db.TrainingAward.Any(f => f.Member.Id == memberId && f.Completed == date));

                var completedBox = find.ById<HtmlInputText>("recordcompleted");
                completedBox.Text = date.ToString("yyyy-MM-dd HH:mm:ss");

                var saveButton = find.ById<HtmlDiv>("editRecordDialog").Parent<HtmlDiv>().Find.ByCustom<HtmlButton>(f => f.InnerText == "Save");
                Assert.IsNotNull(saveButton);
                saveButton.Click();

                while (find.ById<HtmlDiv>("editRecordProcessing").InnerText == "true")
                {
                    browser.RefreshDomTree();
                    System.Threading.Thread.Sleep(500);
                }
                
                var msgP = find.ById<HtmlContainerControl>("editRecordError");
                Assert.IsNotNullOrEmpty(msgP.InnerText);
                Assert.IsTrue(msgP.IsVisible());
            }
        }


        /*
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
 * */
    }
}
