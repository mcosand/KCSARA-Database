using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using Kcsara.Database.Web.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Kcsara.Database.Web.Controllers
{
  public class HomeController : Controller
  {
    private readonly Lazy<IKcsarContext> db;
    public HomeController(Lazy<IKcsarContext> db)
    {
      this.db = db;
    }
    private static string allSearchTypes = string.Join(",", Enum.GetNames(typeof(SearchResultType)));

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult About()
    {
      ViewData["Message"] = "Your application description page.";

      return View();
    }

    public IActionResult Contact()
    {
      ViewData["Message"] = "Your contact page.";

      return View();
    }

    public IActionResult Error()
    {
      return View();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by Angular navigation chrome, /account/detail/{username} link member</remarks>
    /// <param name="q"></param>
    /// <param name="t"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/search")]
    public SearchResult[] ApiSearch(string q, string t = null, int limit = 10)
    {
      var searchTypes = (t ?? allSearchTypes).Split(',').Select(f => (SearchResultType)Enum.Parse(typeof(SearchResultType), f)).ToArray();

      var now = DateTime.Now;
      var last12Months = now.AddMonths(-12);
      var list = new List<SearchResult>();
      if (searchTypes.Any(f => f == SearchResultType.Member))
      {
        list.AddRange(
          MembersController.SummariesWithUnits(
          db.Value.Members.Where(f => (f.FirstName + " " + f.LastName).StartsWith(q)
                             || (f.LastName + ", " + f.FirstName).StartsWith(q)
                             || (f.DEM.StartsWith(q) || f.DEM.StartsWith("SR" + q)))
          .OrderByDescending(f => f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null || g.EndTime > now)))
          .ThenByDescending(f => f.MissionRosters.Count(g => g.TimeIn > last12Months))
          .ThenByDescending(f => f.MissionRosters.Count())
          .ThenBy(f => f.LastName)
          .ThenBy(f => f.FirstName)
          .ThenBy(f => f.Id)).Select((m, i) => new MemberSearchResult
          {
            Score = (m.Units.Length == 0 ? 300 : 1000) - i,
            Summary = m
          }));
      }

      if (searchTypes.Any(f => f == SearchResultType.Mission))
      {
        list.AddRange(
          db.Value.Missions.Where(f => f.StateNumber.StartsWith(q) || f.Title.Contains(q) || f.Location.Contains(q))
          .OrderByDescending(f => f.StartTime)
          .AsEnumerable()
          .Select((f, i) =>
          new MissionSearchResult
          {
            Score = ((f.StartTime > last12Months) ? 500 : 200) - i,
            Summary = new EventSummary
            {
              Id = f.Id,
              StateNumber = f.StateNumber,
              Name = f.Title,
              Location = f.Location,
              Start = f.StartTime,
              Stop = f.StopTime
            }
          })
          );

      }

      return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
    }
  }
}
