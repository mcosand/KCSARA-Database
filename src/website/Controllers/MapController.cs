/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.ViewModels;
  using log4net;
  using Microsoft.AspNet.Mvc;

  public class MapController : BaseController
  {
    public MapController(Lazy<IKcsarContext> dbFactory, ILog log)
      : base(dbFactory, log)
    {
    }

    [HttpGet]
    [Route("api/map/find/{type}")]
    public SearchResult[] ApiFind(string type, string q, int limit = 10)
    {
      return new SearchResult[0];
    }
  }
}
