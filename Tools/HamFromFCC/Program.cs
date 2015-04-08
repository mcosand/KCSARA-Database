/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace HamFromFCC
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Text.RegularExpressions;
  using Kcsar.Database.Data;

  class Program
  {
    static void Main(string[] args)
    {
      using (var db = new KcsarContext())
      {
        DateTime cutoff = DateTime.Today.AddYears(1);

        var course = db.TrainingCourses.Single(f => f.DisplayName == "HAM License");

        var hams = db.PersonContact.Where(f => f.Type == "hamcall")
          .Where(f => !f.Member.ComputedAwards.Any(g => g.Expiry > cutoff && g.Course.Id == course.Id))
          .Select(f => new { Member = f.Member, Call = f.Value })
          .OrderBy(f => f.Member.LastName).ThenBy(f => f.Member.FirstName).ToArray();

        foreach (var ham in hams)
        {
          Console.WriteLine(ham.Call);
          var lic = GetLicense(ham.Call);

          if (lic == null)
          {
            Console.WriteLine("!! No license found");
            continue;
          }

          if (lic.Name.StartsWith(ham.Member.ReverseName, StringComparison.OrdinalIgnoreCase))
          {
            // probably a match
            Console.WriteLine("Grant HAM to {0}, effective {1:yyyy-MM-dd}, expires {2:yyyy-MM-dd}", ham.Member.FullName, lic.Issued, lic.Expires);
            TrainingRecordRow ta = new TrainingRecordRow { Completed = lic.Issued, Course = course, Expiry = lic.Expires, Member = ham.Member, metadata = "Via FCC: " + lic.Url, LastChanged = DateTime.Now, ChangedBy = "HamFromFCC" };
            db.TrainingRecords.Add(ta);
            db.SaveChanges();
            db.RecalculateTrainingAwards(ham.Member.Id);
            db.SaveChanges();
          }
          else
          {
            Console.WriteLine("Can't match names: {0} :: {1}", lic.Name, ham.Member.ReverseName);
          }
        }
      }
    }

    static License GetLicense(string call)
    {
      WebClient client = new WebClient();
      client.Headers.Add(HttpRequestHeader.Referer, "http://wireless2.fcc.gov/UlsApp/UlsSearch/searchAmateur.jsp");
      client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
      string linkPage = client.UploadString("http://wireless2.fcc.gov/UlsApp/UlsSearch/results.jsp", string.Format("fiUlsExactMatchInd=Y&fiulsTrusteeName=&fiOwnerName=&fiUlsFRN=&fiCity=&ulsState=&fiUlsZipcode=&ulsCallSign={0}&statusAll=Y&ulsDateType=&dateSearchType=+&ulsFromDate=&ulsToDate=3%2F8%2F2014&fiRowsPerPage=10&ulsSortBy=uls_l_callsign++++++++++++++++&ulsOrderBy=ASC&x=34&y=11&hiddenForm=hiddenForm&jsValidated=true", call));

      var match = Regex.Match(linkPage, "license.jsp(;JSESSIONID_ULSSEARCH=[a-zA-Z0-9!]+)?\\?licKey=(\\d+)", RegexOptions.IgnoreCase);
      if (!match.Success) return null;

      string url = "http://wireless2.fcc.gov/UlsApp/UlsSearch/license.jsp?licKey=" + match.Groups[2].Value;
      var page = client.DownloadString(url);

      var blockStart = page.IndexOf("<!--Addresses are displayed on successive lines:");
      var blockEnd = page.IndexOf("</td>", blockStart);
      var block = page.Substring(blockStart, blockEnd - blockStart);

      // match = Regex.Match(block, @"^ +([a-zA-Z ,\\-]+)\<br\>");
      match = Regex.Match(page, @"<title>ULS License \- Amateur License \- [A-Z0-9 ]+ \- ([^<]+)</title>");
      if (!match.Success) return null;

      License lic = new License { Name = match.Groups[1].Value };

      match = Regex.Match(page, @"Grant</td>\s+<td[^>]+>\s+([\d/]{10})", RegexOptions.Multiline);
      lic.Issued = DateTime.Parse(match.Groups[1].Value);

      match = Regex.Match(page, @"Expiration</td>\s+<td[^>]+>\s+([\d/]{10})", RegexOptions.Multiline);
      lic.Expires = DateTime.Parse(match.Groups[1].Value);

      lic.Url = url;

      return lic;
    }

    class License
    {
      public string Name;
      public DateTime Issued;
      public DateTime Expires;
      public string Url;

      public override string ToString()
      {
        return string.Format("Issued to {0} on {1}. Expires {2}", Name, Issued, Expires);
      }
    }
  }
}
