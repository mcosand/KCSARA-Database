/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Http;
  using System.Web.Http.ModelBinding.Binders;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.api.Models.Search;
  using log4net;

  [ModelValidationFilter]
  public class SearchController : BaseApiController
  {
    public SearchController(IKcsarContext db, ILog log)
      : base(db, log)
    {
    }

    [HttpGet]
    public List<SearchResult> Find([FromUri(BinderType = typeof(TypeConverterModelBinder))] string q)
    {
      var result = new List<SearchResult>();
      if (!Permissions.IsUser) ThrowAuthError();

      result.AddRange((from m in this.db.Members
                 where (m.FirstName + " " + m.LastName).ToLower().Contains(q)
                   || (m.LastName + ", " + m.FirstName).ToLower().Contains(q)
                   || m.DEM.Contains(q)
                 select m).OrderBy(f => f.LastName).ThenBy(f => f.FirstName).AsEnumerable()
                 .Select(f => new MemberSearchResult(f, Url.Content("~/Members/Detail/") + "{0}", Url.Content(MembersController.PhotosStoreRelativePath)+"{0}", MembersController.StandInPhotoFile))
                 );
      
      return result;
    }
  }
}
