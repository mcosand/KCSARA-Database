/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.ViewModels
{
  public class SearchResponse
  {
    public SearchResult[] Results { get; set; }
  }

  public class SearchResult
  {
    public SearchResultType Type { get; set; }
    internal int Score { get; set; }
  }

  public class MemberSearchResult : SearchResult
  {
    public MemberSearchResult()
    {
      this.Type = SearchResultType.Member;
    }
    public MemberSummary Summary { get; set; }
  }

  public class MissionSearchResult : SearchResult
  {
    public MissionSearchResult()
    {
      this.Type = SearchResultType.Mission;
    }

    public EventSummary Summary { get; set; }
  }

  public enum SearchResultType
  {
    Member,
    Mission
  }
}
