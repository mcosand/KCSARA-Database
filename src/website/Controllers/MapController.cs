/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Mvc;
  using Models;

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
