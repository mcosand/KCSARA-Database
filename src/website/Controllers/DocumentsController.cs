/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Authorization;
  using Microsoft.AspNet.Mvc;
  using Models;
  using Services;

  [Authorize]
  public class DocumentsController : BaseController
  {
    private readonly IDocumentsService service;

    public DocumentsController(Lazy<IDocumentsService> service, Lazy<IKcsarContext> db, ILog log/*, IAppSettings settings*/) : base(db, log/*, settings*/)
    {
      this.service = service.Value;
    }

    [HttpGet]
    [HandleException]
    [Route("/documents/{id}")]
    public ActionResult Get(Guid id)
    {
      var file = service.Get(id);
      return new FileContentResult(file.Data, file.Mime) { FileDownloadName = file.Filename };
    }

    [HttpGet]
    [Route("/documents/{id}/thumbnail")]
    [HandleException]
    public ActionResult Thumbnail(Guid id)
    {
      var thumb = service.GetThumbnail(id);
      return new FileContentResult(thumb.Data, thumb.Mime);
    }

    [HttpGet]
    [HandleException]
    [Route("api/documents/{referenceId}")]
    public IEnumerable<DocumentInfo> ApiDocuments(Guid referenceId)
    {
      return service.List(referenceId);
    }
  }
}