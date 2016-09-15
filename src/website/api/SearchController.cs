/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System.Web.Http;
  using System.Linq;
  using Kcsar.Database.Model;
  using Models;
  using log4net;
  using System.Collections.Generic;
  using System;

  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  [CamelCaseControllerConfig]
  public class SearchController : BaseApiController
  {
    private static string allTypes = string.Join(",", Enum.GetNames(typeof(SearchResultType)));

    public SearchController(IKcsarContext db, Services.IAuthService auth, ILog log)
      : base(db, auth, log)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by Angular navigation chrome, /account/detail/{username} link member</remarks>
    /// <param name="q"></param>
    /// <param name="t"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    [HttpGet]
    public SearchResult[] Search(string q, string t = null, int limit = 10)
    {
      var searchTypes = (t ?? allTypes).Split(',').Select(f => (SearchResultType)Enum.Parse(typeof(SearchResultType), f)).ToArray();

      var now = DateTime.Now;
      var last12Months = now.AddMonths(-12);
      var list = new List<SearchResult>();
      if (searchTypes.Any(f => f == SearchResultType.Member))
      {
        list.AddRange(
          MembersController.SummariesWithUnits(
          db.Members.Where(f => (f.FirstName + " " + f.LastName).StartsWith(q)
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
          db.Missions.Where(f => f.StateNumber.StartsWith(q) || f.Title.Contains(q) || f.Location.Contains(q))
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
