/*
 * Copyright 2008-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Web.Mvc;
  using System.Xml;
  using Database.Extensions;
  using Database.Extensions.Reports;
  using Kcsar.Database;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Services;
  using Kcsara.Database.Web.Model;

  /// <summary>Views related to SAR units.</summary>
  public class UnitsController : BaseController
  {
    private readonly IReportsService reports;
    private readonly Lazy<IExtensionProvider> extensions;

    public UnitsController(Lazy<IExtensionProvider> extensions, IReportsService reports, IKcsarContext db) : base(db)
    {
      this.reports = reports;
      this.extensions = extensions;
    }

    /// <summary>Get a list of existing units.</summary>
    /// <returns>ViewModel</returns>
    [Authorize]
    public ActionResult Index()
    {
      ViewData["Title"] = "Member Units";
      ViewData["Message"] = "Member Units here";

      return View((from u in this.db.Units orderby u.DisplayName select u).AsEnumerable());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">unit id</param>
    /// <returns></returns>
    [Authorize]
    public ActionResult Applicants(Guid id)
    {
      var unit = this.db.Units.Single(f => f.Id == id);
      //var model = this.db.UnitApplicants.Include("Applicant").Where(f => f.Unit.Id == id).OrderBy(f => f.IsActive).ThenBy(f => f.Applicant.LastName).ThenBy(f => f.Applicant.FirstName).AsEnumerable();

      //ViewBag.Unit = unit;
      return View(unit);
    }

    /// <summary>An HTML view of unit members.</summary>
    /// <param name="id">The Unit ID</param>
    /// <returns>ViewModel</returns>
    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Roster(Guid id)
    {


      ViewData["Title"] = "Unit Roster";

      SarUnit unit = (from u in this.db.Units where u.Id == id select u).First();
      ViewData["Unit"] = unit;

      var members = this.db.UnitMemberships.Include("Person").Include("Status").Where(um => um.Unit.Id == id && um.EndTime == null);
      members = members.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

      return View(members);

    }

    public DataActionResult GetMemberEmails(string id)
    {
      if (!Permissions.IsUser) return GetLoginError();

      return Data(UnitsController.GetMemberEmails(this.db, id));
    }

    public static MemberDetailView[] GetMemberEmails(IKcsarContext ctx, string id)
    {
      Guid unitId = UnitsController.ResolveUnit(ctx.Units, id).Id;

      Member[] members = (from m in ctx.GetActiveMembers(unitId, DateTime.Now, "ContactNumbers", "Memberships.Unit", "Memberships.Status") select m).ToArray();
      MemberDetailView[] model = members
          .Where(f => f.Memberships.Any(g => g.Unit.Id == unitId && g.Status.IsActive && g.Status.StatusName != "trainee")
                      && f.ContactNumbers.Count(g => g.Type == "email") > 0)
          .Select(m =>
                      new MemberDetailView
                      {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Contacts = m.ContactNumbers.Where(f => f.Type == "email").Select(f => new MemberContactView { Id = f.Id, Priority = f.Priority, Value = f.Value }).OrderBy(f => f.Priority).ToArray(),
                        Units = m.Memberships.Where(f => f.Status.IsActive && (f.EndTime == null || f.EndTime > DateTime.Now)).Select(f => f.Unit.DisplayName).ToArray()
                      })
          .ToArray();

      return model;
    }

    public ActionResult MissionReadyList(Guid? id)
    {
      if (!Permissions.IsUserOrLocal(Request)) return this.CreateLoginRedirect();

      SarUnit unit = (from u in this.db.Units where u.Id == id select u).FirstOrDefault();

      Stream result = this.reports.GetMissionReadyList(unit);

      return this.File(result, "application/vnd.ms-excel", string.Format("{0}-missionready-{1:yyMMdd}.xls", unit.DisplayName, DateTime.Now));
    }

    [Authorize]
    public ActionResult DownloadGpx(Guid? id)
    {
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<gpx xmlns=""http://www.topografix.com/GPX/1/1"" creator=""KCSARA Database"" version=""1.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd""></gpx>");
      doc.PreserveWhitespace = true;

      string newline = Guid.NewGuid().ToString();

      DateTime now = DateTime.Now;
      var members = this.db.GetActiveMembers(id, now, "Addresses", "ContactNumbers", "Memberships.Unit")
          .Where(f => f.Addresses.Any(g => g.Quality > 0))
          .OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      var model = members.Select(f => new
      {
        Name = f.LastName + ", " + f.FirstName,
        Worker = f.DEM,
        WorkerNumber = f.DEM,
        Numbers = f.ContactNumbers,
        Addresses = f.Addresses,
        Units = f.Memberships.Where(g => g.EndTime == null && g.Status.IsActive).Select(g => g.Unit.DisplayName)
      });

      string gpxx = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";

      foreach (var row in model)
      {
        foreach (var address in row.Addresses.Where(f => f.Quality > 0))
        {
          if (address.Location == null)
          {
            continue;
          }

          var wpt = doc.CreateElement("wpt");
          wpt.SetAttribute("lat", address.Location.Lat.ToString());
          wpt.SetAttribute("lon", address.Location.Long.ToString());
          doc.DocumentElement.AppendChild(wpt);
          var el = doc.CreateElement("name");
          el.InnerText = row.Name;
          wpt.AppendChild(el);
          el = doc.CreateElement("cmt");
          el.InnerText = "DEM# " + row.Worker;
          string ham = row.Numbers.Where(f => f.Type == "hamcall").Select(f => f.Value).FirstOrDefault();
          if (!string.IsNullOrWhiteSpace(ham))
          {
            el.InnerText += newline + "HAM  " + ham;
          }


          wpt.AppendChild(el);

          el = doc.CreateElement("sym");
          el.InnerText = "Residence";
          wpt.AppendChild(el);

          el = doc.CreateElement("extensions");
          wpt.AppendChild(el);

          var ext = doc.CreateElement("gpxx:WaypointExtension", gpxx);
          el.AppendChild(ext);

          el = doc.CreateElement("gpxx:Categories", gpxx);
          ext.AppendChild(el);

          foreach (var unit in row.Units)
          {
            var b = doc.CreateElement("gpxx:Category", gpxx);
            el.AppendChild(b);
            b.InnerText = unit;
          }

          el = doc.CreateElement("gpxx:Address", gpxx);
          ext.AppendChild(el);

          var addr = doc.CreateElement("gpxx:StreetAddress", gpxx);
          addr.InnerText = address.Street;
          el.AppendChild(addr);

          addr = doc.CreateElement("gpxx:City", gpxx);
          addr.InnerText = address.City;
          el.AppendChild(addr);

          addr = doc.CreateElement("gpxx:State", gpxx);
          addr.InnerText = address.State;
          el.AppendChild(addr);

          addr = doc.CreateElement("gpxx:PostalCode", gpxx);
          addr.InnerText = address.Zip;
          el.AppendChild(addr);

          foreach (var phone in row.Numbers.Where(f => f.Type == "phone").OrderBy(f => f.Subtype))
          {
            el = doc.CreateElement("gpxx:PhoneNumber", gpxx);
            ext.AppendChild(el);
            el.SetAttribute("Category", phone.Subtype);
            el.InnerText = phone.Value;
          }
        }
      }

      return new FileContentResult(Encoding.UTF8.GetBytes(doc.OuterXml.Replace(newline, "\n").Replace(" xmlns=\"\"", "")), "text/xml") { FileDownloadName = "members.gpx" };
      //return new ContentResult { Content = doc.OuterXml.Replace(newline, "\n"), ContentType = "text/xml" };
    }

    [Authorize]
    public ActionResult DownloadRoster(Guid? id, bool? includeHidden)
    {
      // The method almost supports id=null as downloading the KCSARA roster
      // It doesn't do a distinct(person), so people in multiple units are recorded more than once.
      ExcelFile xl;
      using (FileStream fs = new FileStream(Server.MapPath(Url.Content("~/Content/roster-template.xls")), FileMode.Open, FileAccess.Read))
      {
        xl = ExcelService.Read(fs, ExcelFileType.XLS);
      }
      ExcelSheet ws = xl.GetSheet(0);

      string filename = string.Format("roster-{0:yyMMdd}.xls", DateTime.Now);
      IQueryable<UnitMembership> memberships = this.db.UnitMemberships.Include("Person").Include("Person.Addresses").Include("Person.ContactNumbers").Include("Status");
      string unitShort = ConfigurationManager.AppSettings["dbNameShort"];
      string unitLong = Strings.GroupName;
      if (id.HasValue)
      {
        memberships = memberships.Where(um => um.Unit.Id == id.Value);
        SarUnit sarUnit = (from u in this.db.Units where u.Id == id.Value select u).First();
        unitShort = sarUnit.DisplayName;
        unitLong = sarUnit.LongName;

      }
      memberships = memberships.Where(um => um.EndTime == null && um.Status.IsActive);
      memberships = memberships.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

      ws.Header = unitLong + " Active Roster";
      ws.Footer = DateTime.Now.ToShortDateString();
      filename = unitShort + "-" + filename;

      using (SheetAutoFitWrapper wrap = new SheetAutoFitWrapper(xl, ws))
      {
        int idx = 1;
        int c = 0;
        foreach (UnitMembership membership in memberships)
        {
          Member member = membership.Person;
          c = 0;
          wrap.SetCellValue(string.Format("{0:0000}", member.DEM), idx, c++);
          wrap.SetCellValue(member.LastName, idx, c++);
          wrap.SetCellValue(member.FirstName, idx, c++);
          wrap.SetCellValue(string.Join("\n", member.Addresses.Select(f => f.Street).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.Addresses.Select(f => f.City).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.Addresses.Select(f => f.State).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.Addresses.Select(f => f.Zip).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.ContactNumbers.Where(f => f.Type == "phone" && f.Subtype == "home").Select(f => f.Value).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.ContactNumbers.Where(f => f.Type == "phone" && f.Subtype == "cell").Select(f => f.Value).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.ContactNumbers.Where(f => f.Type == "email").Select(f => f.Value).ToArray()), idx, c++);
          wrap.SetCellValue(string.Join("\n", member.ContactNumbers.Where(f => f.Type == "hamcall").Select(f => f.Value).ToArray()), idx, c++);
          wrap.SetCellValue(member.WacLevel.ToString(), idx, c++);
          wrap.SetCellValue(membership.Status.StatusName, idx, c++);

          if ((includeHidden ?? false) && (Permissions.IsAdmin || (id.HasValue && Permissions.IsMembershipForUnit(id.Value))))
          {
            wrap.SetCellValue("DOB", 0, c);
            wrap.SetCellValue(string.Format("{0:yyyy-MM-dd}", member.BirthDate), idx, c++);
            wrap.SetCellValue("Gender", 0, c);
            wrap.SetCellValue(member.Gender.ToString(), idx, c++);
          }

          idx++;
        }

        wrap.Sheet.AutoFitAll();
      }

      MemoryStream ms = new MemoryStream();
      xl.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return this.File(ms, "application/vnd.ms-excel", filename);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Detail(Guid id)
    {
      SarUnit unit = (from u in this.db.Units.Include("StatusTypes") where u.Id == id select u).First();

      ViewData["Title"] = "Unit Detail: " + unit.DisplayName;

      Member actor = this.db.Members.Include("Memberships.Status").SingleOrDefault(f => f.Username == User.Identity.Name);

      ViewBag.AppStatus = unit.ApplicationStatus;
      ViewBag.AppText = unit.ApplicationsText;
      ViewBag.CanApply = unit.ApplicationStatus == ApplicationStatus.Yes
          && actor != null
          && !actor.Memberships.Any(f => f.Status.IsActive && f.EndTime == null && f.Unit.Id == unit.Id)
          && !actor.ApplyingTo.Any(f => f.Unit.Id == unit.Id && f.IsActive);
      ViewBag.ActorId = Permissions.UserId;

      ViewBag.CanEditDocuments = api.UnitsController.CanEditDocuments(Permissions, id);

      var unitReports = extensions.Value.For<IUnitReports>(unit);
      ViewBag.UnitReports = (unitReports != null) ? unitReports.ListReports() : new UnitReportInfo[0];

      return View(unit);
    }

    [Authorize]
    public ActionResult DownloadReport(Guid? id, string reportName)
    {
      SarUnit unit = (from u in this.db.Units.Include("StatusTypes") where u.Id == id select u).First();

      var unitReports = extensions.Value.For<IUnitReports>(unit);
      var info = unitReports.ListReports().FirstOrDefault(f => f.Key == reportName);

      MemoryStream ms = new MemoryStream();
      unitReports.RunReport(reportName, ms, Request.QueryString);
      ms.Seek(0, SeekOrigin.Begin);

      return File(ms, info.MimeType, string.Format("{0} {1:yyyy-MM-dd}.{2}", info.Name, DateTime.Now, info.Extension));
    }

    [Authorize]
    public DataActionResult GetStatusTypes(Guid id)
    {
      var result = (from s in this.db.UnitStatusTypes where s.Unit.Id == id orderby s.StatusName select new NameIdViewModel { Name = s.StatusName, Id = s.Id });

      return Data(result);
    }

    #region Unit Status Types
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateStatus(Guid unitId)
    {
      SarUnit unit = (from u in this.db.Units where u.Id == unitId select u).FirstOrDefault();


      ViewData["Title"] = "New Unit Status for " + unit.DisplayName;

      UnitStatus status = new UnitStatus() { Unit = unit };

      Session.Add("NewStatusGuid", status.Id);
      ViewData["NewStatusGuid"] = Session["NewStatusGuid"];

      return InternalEditStatus(status);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateStatus(Guid unitId, FormCollection fields)
    {
      if (Session["NewStatusGuid"] != null && Session["NewStatusGuid"].ToString() != fields["NewStatusGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create a status?");
      }
      Session.Remove("NewStatusGuid");

      SarUnit unit = (from u in this.db.Units where u.Id == unitId select u).FirstOrDefault();
      ViewData["Title"] = "New Unit Status for " + unit.DisplayName;

      UnitStatus status = new UnitStatus();
      status.Unit = unit;
      this.db.UnitStatusTypes.Add(status);
      return InternalSaveStatus(status, fields);
    }

    [Authorize(Roles = "cdb.admins")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult EditStatus(Guid id)
    {
      UnitStatus status = (from s in this.db.UnitStatusTypes.Include("Unit") where s.Id == id select s).First();

      return InternalEditStatus(status);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditStatus(Guid id, FormCollection fields)
    {
      UnitStatus status = (from s in this.db.UnitStatusTypes where s.Id == id select s).FirstOrDefault();

      return InternalSaveStatus(status, fields);
    }

    private ActionResult InternalEditStatus(UnitStatus s)
    {
      List<WacLevel> values = new List<WacLevel>();
      foreach (object o in Enum.GetValues(typeof(WacLevel)))
      {
        values.Add((WacLevel)o);
      }

      ViewData["HideFrame"] = true;
      ViewData["WacLevel"] = new SelectList((from wl in values.Reverse<WacLevel>() select new { Name = wl.ToString(), Value = wl }), "Name", "Value", s.WacLevel);
      return View("EditStatus", s);
    }

    private ActionResult InternalSaveStatus(UnitStatus status, FormCollection fields)
    {
      TryUpdateModel(status, new string[] { "StatusName", "IsActive", "WacLevel", "GetsAccount" });



      //Guid unitId = new Guid(fields["Unit"]);
      //SarUnit unit = (from u in this.db.Units where u.Id == unitId select u).First();
      //um.Unit = unit;

      //Guid statusId = new Guid(fields["Status"]);
      //UnitStatus status = (from s in this.db.SarUnitStatusTypes where s.Id == statusId select s).First();
      //um.Status = status;

      //Guid personId = new Guid(fields["Person"]);
      //Member person = (from m in this.db.Members where m.Id == personId select m).First();
      //um.Status = status;

      if (!ModelState.IsValid)
      {
        return InternalEditStatus(status);
      }

      this.db.SaveChanges();
      return RedirectToAction("ClosePopup");
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteStatus(Guid id)
    {
      ViewData["Title"] = "Delete Unit Status";
      ViewData["HideFrame"] = true;
      return View((from s in this.db.UnitStatusTypes.Include("Unit") where s.Id == id select s).FirstOrDefault());
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteStatus(Guid id, FormCollection fields)
    {
      UnitStatus status = (from s in this.db.UnitStatusTypes where s.Id == id select s).FirstOrDefault();
      this.db.UnitStatusTypes.Remove(status);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }

    #endregion

    internal static SelectList GetUnitSelectList(IKcsarContext local, Guid? selected)
    {
      return new SelectList((from u in local.Units orderby u.DisplayName select new { K = u.Id, N = u.DisplayName }).ToArray(), "K", "N", selected);
    }


    public static SarUnit ResolveUnit(IQueryable<SarUnit> units, string id)
    {
      SarUnit unit = null;
      Guid gID;
      if (Guid.TryParse(id, out gID))
      {
        unit = units.Where(f => f.Id == gID).FirstOrDefault();
      }
      else
      {
        var stringMatch = units.Where(f => f.DisplayName == id);
        if (stringMatch.Count() == 1)
        {
          unit = stringMatch.First();
        }
      }

      if (unit == null)
      {
        throw new ArgumentException("Can't resolve unit '" + id + "'");
      }
      return unit;
    }
  }
}
