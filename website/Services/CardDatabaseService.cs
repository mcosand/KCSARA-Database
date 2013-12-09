using iTextSharp.text;
using iTextSharp.text.pdf;
using Kcsar.Database.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;
using System.Web.Script.Serialization;
using System.Configuration;

namespace Kcsara.Database.Web.Services
{
    public static class CardDatabaseService
    {
        public static List<MemberCardModel> GetEmergencyWorkers()
        {
            WebClient client = new CookiesWebClient();
            string page = client.DownloadString("https://intertraxserver.com/resourcemgrweb/login.aspx");

            var match = Regex.Match(page, "id=\"__VIEWSTATE\" value=\"([^\"]+)\"");
            string viewstate = match.Groups[1].Value;

            match = Regex.Match(page, "id=\"__EVENTVALIDATION\" value=\"([^\"]+)\"");
            string validation = match.Groups[1].Value;

            string foo = "__LASTFOCUS=&__EVENTTARGET=ctl00%24ContentPlaceHolder1%24btnSubmitLink&__EVENTARGUMENT=&__VIEWSTATE="
                + HttpUtility.UrlEncode(viewstate)
                + "&__EVENTVALIDATION="
                + HttpUtility.UrlEncode(validation)
                + "&ctl00%24ContentPlaceHolder1%24txtUsername="
                + HttpUtility.UrlEncode(ConfigurationManager.AppSettings["carddataUser"] ?? "")
                + "&ctl00%24ContentPlaceHolder1%24txtPassword="
                + HttpUtility.UrlEncode(ConfigurationManager.AppSettings["carddataPassword"] ?? "");

            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            page = client.UploadString("https://intertraxserver.com/resourcemgrweb/Login.aspx",foo
                );

            match = Regex.Match(page, "TokenID: '([^']+)'");
            string token = match.Groups[1].Value;

            client.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            page = client.UploadString("https://intertraxserver.com/resourceMgrSvc/Responder/List", "{\"TokenID\":\"" + token + "\",\"Item\":{\"Columns\":[\"FirstName\",\"IdentityCode\",\"LastName\",\"OrganizationName\",\"Rank\"],\"Filter\":[],\"Sort\":[],\"RecordsPerPage\":1,\"Page\":1}}");

            match = Regex.Match(page, "\"total\":(\\d+)");
            string count = match.Groups[1].Value;

            client.Headers.Add(HttpRequestHeader.Accept, "application/json, text/javascript, */*; q=0.01");
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            page = client.UploadString("https://intertraxserver.com/resourceMgrSvc/Responder/List", "{\"TokenID\":\"" + token + "\",\"Item\":{\"Columns\":[\"FirstName\",\"IdentityCode\",\"LastName\",\"OrganizationName\",\"Rank\"],"
                + "\"Filter\":[],"
                + "\"Sort\":[{\"Field\":\"LastName\",\"DescendingOrder\":false},{\"Field\":\"FirstName\",\"DescendingOrder\":false}],"
                + "\"RecordsPerPage\":" + count + ",\"Page\":1}}");
            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Deserialize<ListModel>(page).Data;
        }

        public class ListModel
        {
            public List<MemberCardModel> Data { get; set; }
            public bool Result { get; set; }
            public int records { get; set; }
        }

        public class CookiesWebClient : WebClient
        {
            private CookieContainer cc = new CookieContainer();
            private string lastPage;

            protected override WebRequest GetWebRequest(System.Uri address)
            {
                WebRequest R = base.GetWebRequest(address);
                if (R is HttpWebRequest)
                {
                    HttpWebRequest WR = (HttpWebRequest)R;
                    WR.CookieContainer = cc;
                    if (lastPage != null)
                    {
                        WR.Referer = lastPage;
                    }
                }
                lastPage = address.ToString();
                return R;
            }
        }
    }

    public class MemberCardModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private string dem;
        public string IdentityCode
        {
            get { return dem.PadLeft(4, '0'); }
            set { dem = value; }
        }
        public string Rank { get; set; }
        public int PK { get; set; }
        public override string ToString()
        {
            return string.Format("{0} {1} ({2}), {3}", FirstName, LastName, IdentityCode, Rank);
        }
    }
}