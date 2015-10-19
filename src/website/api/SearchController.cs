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
    public SearchController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpGet]
    public SearchResult[] Search(string q, SearchResultType[] t, int limit = 10)
    {
      var last12Months = DateTime.Now.AddMonths(-12);
      var list = new List<SearchResult>();
      if (t == null || t.Any(f => f == SearchResultType.Member))
      {
        list.AddRange(
          MembersController.SummariesWithUnits(
          db.Members.Where(f => f.FirstName.StartsWith(q) || f.LastName.StartsWith(q))
          .OrderByDescending(f => f.MissionRosters.Count(g => g.TimeIn > last12Months))
          .ThenByDescending(f => f.MissionRosters.Count())
          .ThenBy(f => f.LastName)
          .ThenBy(f => f.FirstName)
          .ThenBy(f => f.Id)).Select((m, i) => new MemberSearchResult
          {
            Score = 1000 - i,
            Summary = m
          }));
      }

      return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
    }
  }
}
