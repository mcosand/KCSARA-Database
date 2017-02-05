using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Sar.Database.Model;
using Sar.Database.Model.Search;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Units
{
  public class SearchController
  {
    public class RootController : ApiController
    {
      private readonly ILog _log;
      private readonly IMissionsService _missions;
      private readonly IMembersService _members;

      public RootController(IMissionsService missions, IMembersService members, IAuthorizationService authz, ILog log)
      {
        _missions = missions;
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
          list.AddRange(await _missions.SearchMissionsAsync(q));
        }

        return list.OrderByDescending(f => f.Score).Take(limit).ToArray();
      }
    }
  }
}
