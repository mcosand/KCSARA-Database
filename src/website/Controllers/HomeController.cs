/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Models;
  using Microsoft.AspNet.Mvc;
  using Services;
  public class HomeController : Controller
  {
    private readonly Lazy<IMembersService> members;
    private readonly Lazy<IEventsService<Mission>> missions;

    public HomeController(Lazy<IMembersService> members, Lazy<IEventsService<Mission>> missions)
    {
      this.members = members;
      this.missions = missions;
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
          members.Value
          .Search(q)
          .Select((m, i) => new MemberSearchResult
          {
            Score = (m.Units.Length == 0 ? 300 : 1000) - i,
            Summary = m
          }));
      }

      if (searchTypes.Any(f => f == SearchResultType.Mission))
      {
        list.AddRange(
          missions.Value
          .Search(q)
          .Select((m, i) => new MissionSearchResult
          {
            Score = ((m.Start > last12Months) ? 500 : 200) - i,
            Summary = m
          }));
      }

      return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
    }
  }
}
