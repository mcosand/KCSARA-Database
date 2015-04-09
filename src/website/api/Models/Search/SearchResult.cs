/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api.Models.Search
{
  using System;

  public class SearchResult
  {
    public SearchResult(SearchResultType type, string text, string url)
    {
      this.Type = type;
      this.Text = text;
      this.Url = url;
    }

    public SearchResultType Type { get; set; }
    public string Text { get; set; }
    public string Url { get; set; }
  }

  public class SearchResult<T> : SearchResult
  {
    public SearchResult(SearchResultType type, string text, string url, T more)
      : base(type, text, url)
    {
      this.More = more;
    }

    public T More { get; set; }
  }
}