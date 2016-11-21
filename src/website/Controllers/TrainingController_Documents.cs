/*
 * Copyright 2011-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Drawing;
  using System.Drawing.Imaging;
  using System.Linq;
  using System.Web;
  using System.Web.Mvc;
  using System.Windows.Media.Imaging;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web;
  using Kcsara.Database.Web.Model;
  using System.Collections.Generic;
  using System.IO;
  using System.Data.Entity;

  public partial class TrainingController
  {
    [AuthorizeWithLog]
    public ActionResult Documents(string id)
    {
      Guid mid;
      if (!Guid.TryParse(id, out mid)) return new ContentResult { Content = "Bad training Id" };

      var model = this.db.Trainings.FirstOrDefault(f => f.Id == mid);
      if (model == null) return new ContentResult { Content = "Training not found" };

      ViewData["trainingId"] = mid;


      return View(model);
    }

    [HttpPost]
    public DataActionResult GetTrainingDocuments(Guid id)
    {
      if (!Permissions.IsUser) return GetLoginError();

      DocumentView[] model = (from d in this.db.Documents
                              where d.ReferenceId == id
                              orderby d.FileName
                              select new DocumentView
                              {
                                Id = d.Id,
                                Title = d.FileName,
                                Size = d.Size,
                                Type = d.Type,
                                Mime = d.MimeType
                              }).ToArray();

      return Data(model);
    }

    [AuthorizeWithLog(Roles = "cdb.trainingeditors")]
    [HttpGet]
    public ActionResult UploadDocument(Guid id)
    {
      return View(this.db.Trainings.First(f => f.Id == id));
    }

    [AuthorizeWithLog(Roles = "cdb.trainingeditors")]
    [HttpPost]
    public ActionResult UploadDocument(Guid id, FormCollection fields)
    {
      Kcsara.Database.Web.Documents.ReceiveDocuments(Request.Files, this.db, id, "unknown");
      this.db.SaveChanges();
      return ClosePopup();
    }

    [HttpPost]
    public DataActionResult DeleteDocument(Guid id)
    {
      if (!User.IsInRole("cdb.trainingeditors"))
      {
        return GetLoginError();
      }

      var doc = (from w in this.db.Documents where w.Id == id select w).First();
      this.db.Documents.Remove(doc);
      this.db.SaveChanges();

      return Data(new SubmitResult<bool> { Result = true, Errors = new SubmitError[0] });
    }

    public DataActionResult RotateImage(Guid? id, bool? clockwise)
    {
      if (!User.IsInRole("cdb.trainingeditors"))
      {
        return GetLoginError();
      }

      var doc = (from d in this.db.Documents where d.Id == id.Value select d).First();
      doc.Contents = Kcsara.Database.Web.Documents.RotateImage(doc.Contents, clockwise);
      ((KcsarContext)this.db).Entry(doc).State = EntityState.Modified;
      this.db.SaveChanges();

      return Data(true);
    }

    public DataActionResult RecentDocuments(DateTime since)
    {
      if (!User.IsInRole("cdb.users")) return GetLoginError();

      List<RecentDocumentsView> result = new List<RecentDocumentsView>();
      var results = (from u in this.db.Members
                      join ta in this.db.TrainingAward on u.Id equals ta.Member.Id
                      join tc in this.db.TrainingCourses on ta.Course.Id equals tc.Id
                      join d in this.db.Documents on ta.Id equals d.ReferenceId
                      where (tc.ShowOnCard || tc.WacRequired > 0) && (ta.Completed >= since || d.LastChanged >= since)
                      select new { u.FirstName, u.LastName, tc.DisplayName, ta.Completed, d.FileName, DocumentId = d.Id });

      foreach (var row in results)
      {
        string destFile;
        int i = 0;
        do
        {
          destFile = string.Format("{0}, {1}_{2}_{3:yyyy-MM-dd}{4}{5}",
              row.LastName,
              row.FirstName,
              row.DisplayName,
              row.Completed,
              (i == 0) ? "" : ("-" + i.ToString()),
              Path.GetExtension(row.FileName));
          i++;
        } while (result.Any(f => f.Filename == destFile) && i < 100);
        result.Add(new RecentDocumentsView { Filename = destFile, DownloadUrl = Url.Action("DownloadDoc", new { id = row.DocumentId }) });
      }

      return Data(result.OrderBy(f => f.Filename).ToArray());
    }

    protected override bool Has4x4RosterPermissions()
    {
      return User.IsInRole("cdb.trainingeditors");
    }

    protected override void AddRosterRowFrom4x4Sheet(ExpandedRowsContext model, SarUnit unit, IRosterEntry row)
    {
      TrainingRoster mrow = (TrainingRoster)row;

      Guid personId = mrow.Person.Id;
      TrainingRoster newRow = new TrainingRoster
      {
        Training = (Training)model.SarEvent,
        Person = this.db.Members.Include("TrainingRosters").Single(f => f.Id == personId),
        TimeIn = mrow.TimeIn,
        TimeOut = mrow.TimeOut,
        Miles = mrow.Miles,
      };
      mrow.Id = newRow.Id;
      this.db.TrainingRosters.Add(newRow);
    }
  }
}
