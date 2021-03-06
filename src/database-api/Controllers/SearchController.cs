﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Sar;
using Sar.Database;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using Sar.Database.Model.Search;
using Sar.Database.Model.Units;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class SearchController
  {
    public class RootController : ApiController
    {
      private readonly ILog _log;
      private readonly IEventsService _events;
      private readonly IMembersService _members;

      public RootController(IEventsService events, IMembersService members, IAuthorizationService authz, ILog log)
      {
        _events = events;
        _members = members;
        _authz = authz;
        _log = log;
      }

      private static string allTypes = string.Join(",", Enum.GetNames(typeof(SearchResultType)));
      private readonly IAuthorizationService _authz;

      /// <summary>
      /// 
      /// </summary>
      /// <remarks>used by Angular navigation chrome, /account/detail/{username} link member</remarks>
      /// <param name="q"></param>
      /// <param name="t"></param>
      /// <param name="limit"></param>
      /// <returns></returns>
      [HttpGet]
      [Route("search")]
      [AnyHostCorsPolicy]
      public async Task<SearchResult[]> Search(string q, string t = null, int limit = 10)
      {
        var searchTypes = (t ?? allTypes).Split(',').Select(f => (SearchResultType)Enum.Parse(typeof(SearchResultType), f)).ToArray();

        var now = DateTime.Now;
        var last12Months = now.AddMonths(-12);
        var list = new List<SearchResult>();
        if (searchTypes.Any(f => f == SearchResultType.Member) &&  await _authz.AuthorizeAsync(null, "Read:Member"))
        {
          list.AddRange(await _members.SearchAsync(q));
        }

        if (searchTypes.Any(f => f == SearchResultType.Mission) && await _authz.AuthorizeAsync(null, "Read:Mission"))
        {
          list.AddRange(await _events.SearchMissionsAsync(q));
        }

        return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
      }
    }
  }
}
