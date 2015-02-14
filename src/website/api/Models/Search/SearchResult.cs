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
    public Guid Id { get; set; }
    public string Text { get; set; }
    public string Url { get; set; }
  }
}