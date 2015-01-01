/*
 * Copyright 2011-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Linq;
  using System.Web.Mvc;
  using Kcsara.Database.Web.Model;

  public partial class AdminController
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    [Authorize(Roles = "cdb.admins")]
    public ActionResult SubmitDocument()
    {
      return View();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    [Authorize(Roles = "cdb.submitdocuments")]
    [HttpPost]
    public ActionResult SubmitDocument(FormCollection fields)
    {
      Guid reference = Documents.SubmittedDocument;
      string type = "unknown";

      Documents.ReceiveDocuments(Request.Files, this.db, reference, type);
      this.db.SaveChanges();

      return ClosePopup();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ActionResult PendingDocuments()
    {
      return View();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public DataActionResult GetPendingDocuments()
    {
      if (!Permissions.IsUser) return GetLoginError();

      DocumentView[] model;
      model = (from d in this.db.Documents
                where d.ReferenceId == Documents.SubmittedDocument
                select new DocumentView
                    {
                      Id = d.Id,
                      Mime = d.MimeType,
                      Changed = d.LastChanged,
                      Title = d.FileName,
                      Size = d.Size
                    }).ToArray();

      return Data(model);
    }

    [HttpPost]
    public DataActionResult DeleteDocument(Guid id)
    {
      if (!User.IsInRole("cdb.admins"))
      {
        return GetLoginError();
      }

      var doc = (from w in this.db.Documents where w.Id == id select w).First();
      this.db.Documents.Remove(doc);
      this.db.SaveChanges();

      return Data(new SubmitResult<bool> { Result = true, Errors = new SubmitError[0] });
    }
  }
}
