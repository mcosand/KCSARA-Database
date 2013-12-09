using Kcsar.Database.Model;
using Kcsara.Database.Web;
using Kcsara.Database.Web.api;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Kcsara.Database.Website.Tests.Apis
{
    [TestFixture]
    public class TrainingRecordTests
    {
        [Test]
        public void GetById()
        {
            Guid id;
            using (var db = new KcsarContext())
            {
                id = db.TrainingAward.First().Id;
            }

            string url = "/api/TrainingRecords/Get/" + id.ToString();
            Console.WriteLine(url);
            string content = DatabaseAutomation.DownloadPageAsAdmin(url);
            Console.WriteLine(content);
        }

        [Test]
        public void Post_Duplicate()
        {
            string post;
            using (var db = new KcsarContext())
            {
                var existing = db.TrainingAward.First();
                post = string.Format(
                                    @"{{""Course"":{{""Id"":""{0}""}},""Member"":{{""Id"":""{1}""}},""Completed"":""{2}"",""ExpirySrc"":""default""}}",
                                    existing.Course.Id,
                                    existing.Member.Id,
                                    existing.Completed);
            }

            try
            {
                DatabaseAutomation.PostJsonAsAdmin("/api/TrainingRecords/Post", post);
                Assert.Fail("Request should have failed");
            }
            catch (WebException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode, "Status code");
                string response = DatabaseAutomation.ReadExceptionResponse(e);
                Console.WriteLine(response);
                var errors = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                Assert.AreEqual(1, errors.Count, "Count");
                Assert.AreEqual(Strings.API_TrainingRecord_Duplicate, errors[BaseApiController.ModelRootNodeName], "error text");
            }
        }

        [Test]
        public void Post_MemberNotFound()
        {
            string post;
            using (var db = new KcsarContext())
            {
                var existing = db.TrainingAward.First();
                post = string.Format(
                                    @"{{""Course"":{{""Id"":""{0}""}},""Member"":{{""Id"":""{1}""}},""Completed"":""{2}"",""ExpirySrc"":""default""}}",
                                    existing.Course.Id,
                                    Guid.NewGuid(),
                                    existing.Completed);
            }

            try
            {
                DatabaseAutomation.PostJsonAsAdmin("/api/TrainingRecords/Post", post);
                Assert.Fail("Request should have failed");
            }
            catch (WebException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, ((HttpWebResponse)e.Response).StatusCode, "Status code");
                string response = DatabaseAutomation.ReadExceptionResponse(e);
                Console.WriteLine(response);
                var errors = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                Assert.AreEqual(1, errors.Count, "Count");
                Assert.AreEqual("Not found", errors["Member"], "error text");
            }
        }
    }
}
