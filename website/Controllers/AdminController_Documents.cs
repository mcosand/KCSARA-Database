namespace Kcsara.Database.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Kcsara.Database.Web.Model;
    using System.Data.Objects.SqlClient;

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

            using (var ctx = GetContext())
            {
                Documents.ReceiveDocuments(Request.Files, ctx, reference, type);
                ctx.SaveChanges();
            }
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
            using (var ctx = GetContext())
            {
                model = (from d in ctx.Documents
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
        }

        [HttpPost]
        public DataActionResult DeleteDocument(Guid id)
        {
            if (!User.IsInRole("cdb.admins"))
            {
                return GetLoginError();
            }

            using (var ctx = GetContext())
            {
                var doc = (from w in ctx.Documents where w.Id == id select w).First();
                ctx.Documents.Remove(doc);
                ctx.SaveChanges();
            }

            return Data(new SubmitResult<bool> { Result = true, Errors = new SubmitError[0] });
        }
    }
}
