/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Drawing;
  using System.Drawing.Imaging;
  using System.Linq;
  using System.Text;
  using System.Web;
  using System.Web.Mvc;
  using System.Windows.Media.Imaging;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Model;
  using iTextSharp.text.pdf;
  using System.IO;
  using iTextSharp.text;

  public partial class MissionsController
  {
    [Authorize]
    public ActionResult Documents(string id)
    {
      Guid mid;
      if (!Guid.TryParse(id, out mid)) return new ContentResult { Content = "Bad mission Id" };

      var model = this.db.Missions.FirstOrDefault(f => f.Id == mid);
      if (model == null) return new ContentResult { Content = "Mission not found" };

      ViewData["missionId"] = mid;


      return View(model);
    }


    public ActionResult ICS109(Guid id)
    {
      var mission = this.db.Missions.Include("Log").Single(f => f.Id == id);


      string pdfTemplate = Server.MapPath(Url.Content("~/Content/forms/ics109-log.pdf"));


      using (MemoryStream result = new MemoryStream())
      {
        iTextSharp.text.Document resultDoc = new iTextSharp.text.Document();
        PdfCopy copy = new PdfCopy(resultDoc, result);
        resultDoc.Open();

        Queue<Tuple<string, string, string>> rows = null;
        int numPages = -1;
        int totalRows = 0;
        int page = 1;

        List<string> operators = new List<string>();

        do
        {
          using (MemoryStream filledForm = new MemoryStream())
          {
            iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(pdfTemplate);
            //// create and populate a string builder with each of the 
            //// field names available in the subject PDF

            //StringBuilder sb = new StringBuilder();
            //foreach (var de in pdfReader.AcroFields.Fields)
            //{
            //    sb.Append(de.Key.ToString() + Environment.NewLine);
            //}
            //// Write the string builder's content to the form's textbox

            using (MemoryStream buf = new MemoryStream())
            {
              PdfStamper stamper = new PdfStamper(pdfReader, buf);

              var fields = stamper.AcroFields;

              if (rows == null)
              {
                rows = Fill109Rows(mission.Log.OrderBy(f => f.Time), fields, "topmostSubform[0].Page1[0].SUBJECTRow1[0]");
                totalRows = rows.Count;
              }

              foreach (var field in fields.Fields)
              {
                fields.SetField(field.Key, "");
              }

              int currentRow = 1;
              operators.Clear();
              while (rows.Count > 0 && fields.GetField("topmostSubform[0].Page1[0].SUBJECTRow" + currentRow.ToString() + "[0]") != null)
              {
                var row = rows.Dequeue();

                fields.SetField("topmostSubform[0].Page1[0].TIMERow" + currentRow.ToString() + "[0]", row.Item1);
                fields.SetField("topmostSubform[0].Page1[0].SUBJECTRow" + currentRow.ToString() + "[0]", row.Item2);

                if (!operators.Contains(row.Item3)) operators.Add(row.Item3);
                currentRow++;
              }

              // Now we know how many rows on a page. Figure out how many pages we need for all rows.
              if (numPages < 0)
              {
                int rowsPerPage = currentRow - 1;
                int remainder = totalRows % currentRow;
                numPages = ((remainder == 0) ? 0 : 1) + (totalRows / currentRow);
              }

              if (numPages > 0)
              {
                fields.SetField("topmostSubform[0].Page1[0]._1_Incident_Name[0]", "   " + mission.Title);
                fields.SetField("topmostSubform[0].Page1[0]._3_DEM_KCSO[0]", "    " + mission.StateNumber);
                fields.SetField("topmostSubform[0].Page1[0]._5_RADIO_OPERATOR_NAME_LOGISTICS[0]", string.Join(",", operators.Distinct()));
                fields.SetField("topmostSubform[0].Page1[0].Text30[0]", string.Format("{0:yyyy-MM-dd}", mission.Log.DefaultIfEmpty().Min(f => (f == null) ? (DateTime?)null : f.Time)));
                fields.SetField("topmostSubform[0].Page1[0].Text31[0]", string.Format("{0:yyyy-MM-dd}", mission.Log.DefaultIfEmpty().Max(f => (f == null) ? (DateTime?)null : f.Time)));
                fields.SetField("topmostSubform[0].Page1[0].Text28[0]", page.ToString());
                fields.SetField("topmostSubform[0].Page1[0].Text29[0]", numPages.ToString());
                fields.SetField("topmostSubform[0].Page1[0].DateTime[0]", DateTime.Now.ToString("     MMM d, yyyy  HH:mm"));
                fields.SetField("topmostSubform[0].Page1[0]._8_Prepared_by_Name[0]", Strings.DatabaseName);

                fields.RemoveField("topmostSubform[0].Page1[0].PrintButton1[0]");
              }

              stamper.FormFlattening = false;
              stamper.Close();

              pdfReader = new PdfReader(buf.ToArray());
              copy.AddPage(copy.GetImportedPage(pdfReader, 1));
              page++;
            }
          }
          //copy.Close();
        } while (rows != null && rows.Count > 0);

        resultDoc.Close();
        return File(result.ToArray(), "application/pdf", mission.StateNumber + "_ICS109_CommLog.pdf");
      }
    }

    Queue<Tuple<string, string, string>> Fill109Rows(IEnumerable<MissionLog> logs, AcroFields fields, string fieldName)
    {
      AcroFields.Item item = fields.GetFieldItem(fieldName);
      PdfDictionary merged = item.GetMerged(0);
      TextField textField = new TextField(null, null, null);
      fields.DecodeGenericDictionary(merged, textField);

      var collection = new Queue<Tuple<string, string, string>>();
      float fieldWidth = fields.GetFieldPositions(fieldName)[0].position.Width;
      float padRight = textField.Font.GetWidthPoint("m", textField.FontSize);

      foreach (var log in logs)
      {
        if (log.Data == null) continue;

        string formTime = log.Time.ToString("HHmm");
        string formOperator = (log.Person ?? new Member()).LastName;

        foreach (var logMsg in log.Data.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
          int left = 0;
          int right = logMsg.Length - 1;

          while (left < right)
          {
            string part = logMsg.Substring(left, right - left + 1);
            while (left < right && (textField.Font.GetWidthPoint(part, textField.FontSize) + padRight) > fieldWidth)
            {
              right = left + part.LastIndexOf(' ');
              part = logMsg.Substring(left, right - left);
            }
            collection.Enqueue(new Tuple<string, string, string>(formTime, part, formOperator));
            formTime = "";
            left = right;
            right = logMsg.Length - 1;
          }
        }
      }

      return collection;
    }


    //public ActionResult ICS109(Guid id)
    //{
    //    var model = BuildRosterModel(id);
    //    using (KcsarContext ctx = GetContext())
    //    {
    //        Dictionary<Guid, string> numbers = new Dictionary<Guid, string>();
    //        foreach (var row in ctx.MissionRosters.Where(f => f.Mission.Id == id).Select(f => f.Person.Id).Distinct())
    //        {
    //            numbers.Add(row, ctx.PersonContact.Where(f => f.Person.Id == row && f.Subtype == "cell").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault());
    //        }
    //        ViewData["phones"] = numbers;
    //    }
    //    return View(model);
    //}

    [HttpPost]
    public DataActionResult GetMissionDocuments(Guid id)
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

    [Authorize(Roles = "cdb.missioneditors")]
    [HttpGet]
    public ActionResult UploadDocument(Guid id)
    {
      return View(this.db.Missions.First(f => f.Id == id));
    }

    [Authorize(Roles = "cdb.missioneditors")]
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
      if (!User.IsInRole("cdb.missioneditors"))
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
      if (!User.IsInRole("cdb.missioneditors"))
      {
        return GetLoginError();
      }

      var doc = (from d in this.db.Documents where d.Id == id.Value select d).First();
      doc.Contents = Kcsara.Database.Web.Documents.RotateImage(doc.Contents, clockwise);
      this.db.Entry(doc).State = EntityState.Modified;
      this.db.SaveChanges();

      return Data(true);
    }

    protected override bool Has4x4RosterPermissions()
    {
      return User.IsInRole("cdb.missioneditors");
    }

    protected override void AddRosterRowFrom4x4Sheet(ExpandedRowsContext model, SarUnit unit, IRosterEntry row)
    {
      MissionRoster_Old mrow = (MissionRoster_Old)row;

      Guid personId = mrow.Person.Id;
      MissionRoster_Old newRow = new MissionRoster_Old
      {
        Mission = (Mission_Old)model.SarEvent,
        Unit = unit,
        Person = this.db.Members.Include("MissionRosters").Single(f => f.Id == personId),
        TimeIn = mrow.TimeIn,
        TimeOut = mrow.TimeOut,
        Miles = mrow.Miles,
        InternalRole = mrow.InternalRole
      };
      mrow.Id = newRow.Id;
      this.db.MissionRosters.Add(newRow);
    }
  }
}
