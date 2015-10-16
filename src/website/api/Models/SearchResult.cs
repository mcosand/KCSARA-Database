/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models
{
  public class SearchResult
  {
    public SearchResultType Type { get; set; }

  }

  public enum SearchResultType
  {
    Member,
    Mission
  }
}
