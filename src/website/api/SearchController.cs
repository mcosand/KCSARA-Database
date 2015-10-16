/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System.Web.Http;
  using Kcsar.Database.Model;
  using Models;
  using log4net;

  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  public class SearchController : BaseApiController
  {
    public SearchController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpGet]
    public SearchResult[] Search(string q, SearchResultType[] t, int? limit = 10)
    {
      return new[] { new SearchResult { Type = SearchResultType.Member }, new SearchResult { Type = SearchResultType.Mission } };
    }
  }
}
