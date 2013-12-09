using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database.Website.Tests
{
    public static class DatabaseAutomation
    {
        public static Uri GetDatabaseUrl()
        {
            return new Uri("http://localhost:7031/");
        }

        public static NetworkCredential GetAdminCredential()
        {
            return new NetworkCredential("admin", "notasecurepassword");
        }

        public static string GetMailDrop()
        {
            return "c:\\maildrop";
        }

        public static string DownloadPageAsAdmin(string relativePath)
        {
            relativePath += relativePath.Contains("?") ? "&_auth=basic" : "?_auth=basic";

            WebClient client = new WebClient();
            client.Credentials = DatabaseAutomation.GetAdminCredential();
            return client.DownloadString(new Uri(DatabaseAutomation.GetDatabaseUrl(), relativePath));
        }

        public static string PostJsonAsAdmin(string relativePath, string post)
        {
            relativePath += relativePath.Contains("?") ? "&_auth=basic" : "?_auth=basic";

            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            client.Credentials = DatabaseAutomation.GetAdminCredential();
            return client.UploadString(new Uri(DatabaseAutomation.GetDatabaseUrl(), relativePath), post);
        }

        public static string ReadExceptionResponse(WebException exception)
        {
            Stream response = exception.Response.GetResponseStream();
            byte[] buffer = new byte[response.Length];
            response.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer);
        }
    }
}

