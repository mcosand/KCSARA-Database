/*
 * Copyright 2012-2014 Matthew Cosand
 */
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Website.Tests
{
    [TestFixture]
    public class SiteAdminTests
    {
        [Test]
        public void ErrorSendsEmail()
        {
            string[] olderFiles = Directory.GetFiles(DatabaseAutomation.GetMailDrop());
            try
            {
                DatabaseAutomation.DownloadPageAsAdmin("Admin/TestExceptionHandling");
            }
            catch (WebException ex)
            {
                Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status, "Should get error from downloading page");
            }
            string[] newFiles = Directory.GetFiles(DatabaseAutomation.GetMailDrop());

            foreach (string file in newFiles.Except(olderFiles))
            {
                File.Delete(file);
            }

            Assert.AreEqual(olderFiles.Length + 1, newFiles.Length, "Should have sent one email based on error");
        }
    }
}
