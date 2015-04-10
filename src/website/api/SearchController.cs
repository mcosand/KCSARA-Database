/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Http;
  using System.Web.Http.ModelBinding.Binders;
  using Kcsara.Database.Model.Members;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.api.Models;
  using Kcsara.Database.Web.api.Models.Search;
  using log4net;

  [ModelValidationFilter]
  public class SearchController : NoDataBaseApiController
  {
    protected readonly IMembersService membersSvc;

    public SearchController(IMembersService membersSvc, ILog log)
      : base(log)
    {
      this.membersSvc = membersSvc;
    }

    [HttpGet]
    public List<SearchResult> Find([FromUri(BinderType = typeof(TypeConverterModelBinder))] string q)
    {
      var result = new List<SearchResult>();
      if (!Permissions.IsUser) ThrowAuthError();

      result.AddRange(
        membersSvc
          .Search(q)
          .Select(f => {
            return new SearchResult<MemberSummary>(
              SearchResultType.Member,
              f.Name,
              Url.Link("Default", new { controller = "Members", action = "Detail", id = f.Id }),
              f);
            })
      );

      return result;
    }
  }
}
