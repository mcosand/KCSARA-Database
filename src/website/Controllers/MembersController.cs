/*
 * Copyright 2008-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using Kcsar.Database;
  using Kcsar.Database.Model;
  using Kcsara.Database.Geo;
  using Kcsara.Database.Web.Model;
  using ApiModels = Kcsara.Database.Web.api.Models;
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Drawing.Imaging;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Text;
  using System.Web;
  using System.Web.Mvc;
  using IO = System.IO;
  using Kcsara.Database.Web.Services;
  using System.Data.Entity.Validation;

  public class MembersController : BaseController
  {
    public MembersController(IKcsarContext db) : base(db) { }

    [Authorize]
    //        [FilterTypes(FilterTypes.ActiveOnly | FilterTypes.Time | FilterTypes.Unit)]
    public ActionResult Index()
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      ViewData["PageTitle"] = "Members' Index";
      ViewData["Message"] = "Members Index Page";

      DateTime now = DateTime.Now;
      var members = this.db.GetActiveMembers(null, now, "Memberships.Unit", "Memberships.Status");
      var model = members.ToArray().Select(f => new ApiModels.MemberSummary(f)
      {
        Units = f.Memberships.Where(this.db.GetActiveMembershipFilter(null, now)).ToDictionary(g => g.Unit.Id, g => g.Unit.DisplayName)
      });

      return View(model);
    }

    /// <summary>
    /// Retrieves all of the details for a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult Detail(Guid id)
    {
      if (!(Permissions.IsUser || Permissions.IsSelf(id))) return this.CreateLoginRedirect();

      ViewData["CanEditSelf"] = Permissions.IsSelf(id) || Permissions.IsMembershipForPerson(id) || Permissions.IsAdmin;
      ViewData["CanEditMember"] = Permissions.IsAdmin || Permissions.IsMembershipForPerson(id);
      ViewData["CanEditPhoto"] = Permissions.IsAdmin || Permissions.IsMembershipForPerson(id) || Permissions.IsInRole(new[] { "cdb.photos" });
      ViewData["CanEditAdmin"] = Permissions.IsAdmin;

      ViewData["CanPrintBadge"] = User.IsInRole("cdb.badges");
      ViewData["IsUser"] = Permissions.IsUser;
      ViewData["IsSelf"] = Permissions.IsSelf(id);

      ViewData["HideMenu"] = Permissions.IsUser ? null : new object();

      Member member = (from m in this.db.Members.Include("Memberships.Unit").Include("Memberships.Status")
              .Include("Addresses").Include("MissionRosters").Include("MissionRosters.Mission").Include("MissionRosters.Unit")
              .Include("TrainingRosters").Include("TrainingRosters.Training").Include("Animals").Include("Animals.Animal")
                       where m.Id == id
                       select m).FirstOrDefault();

      ViewData["isApplicant"] = member.ApplyingTo.Count > 0; //(member.Status & MemberStatus.Applicant) == MemberStatus.Applicant;
      ViewData["Title"] = member.FullName + " :: KCSARA Database";

      if (!User.IsInRole("cdb.admins"))
      {
        FilterPersonal(member);
      }

      var courses = this.db.GetCoreCompetencyCourses();
      ViewData["CoreStatus"] = CompositeTrainingStatus.Compute(member, courses, DateTime.Now).Expirations.ToDictionary(f => f.Value.CourseName, f => f.Value);
      return View(member);
    }


    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Awards(Guid id)
    {
      Member member = GetMember(this.db.Members.Include("TrainingAwards").Include("TrainingAwards.Course"), id);

      return View(member);
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize]
    public ActionResult NewEsarTrainee(DateTime? date, string last)
    {
      if (!Permissions.IsAdmin && !Permissions.IsInRole("esar.trainingdirectors")) return this.CreateLoginRedirect();

      if (date.HasValue)
      {
        ViewData["CourseDate"] = date;
      }

      if (!string.IsNullOrEmpty(last))
      {
        ViewData["success"] = "Saved " + last;
      }
      ViewData["Gender"] = new SelectList(Enum.GetNames(typeof(Gender)), Gender.Unknown.ToString());
      ViewData["Title"] = "New ESAR Trainee";
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult NewEsarTrainee(FormCollection fields)
    {
      if (!Permissions.IsAdmin && !Permissions.IsInRole("esar.trainingdirectors")) return this.CreateLoginRedirect();

      Member m = NewEsarTrainee_Internal(fields);
      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        this.db.RecalculateTrainingAwards(m.Id);
        this.db.SaveChanges();

        return RedirectToAction("NewEsarTrainee", new { date = fields["CourseDate"], last = m.FullName });
      }

      ViewData["Gender"] = new SelectList(Enum.GetNames(typeof(Gender)), ((m == null) ? Gender.Unknown : m.Gender).ToString());
      ViewData["Title"] = "New ESAR Trainee";

      return View();
    }

    private Member NewEsarTrainee_Internal(FormCollection fields)
    {
      Member m = new Member();
      TryUpdateModel(m, new[] { "FirstName", "LastName", "MiddleName", "BirthDate", "SheriffApp", "Gender" });
      this.db.Members.Add(m);

      SarUnit esar = (from u in this.db.Units where u.DisplayName == "ESAR" select u).First();
      UnitStatus status = (from s in this.db.UnitStatusTypes where s.Unit.Id == esar.Id && s.StatusName == "trainee" select s).First();

      if (!string.IsNullOrEmpty(fields["Street"]))
      {
        PersonAddress address = new PersonAddress { Person = m, Type = PersonAddressType.Mailing };
        TryUpdateModel(address, new[] { "Street", "City", "State" });

        GeographyServices.RefineAddressWithGeography(address);
        if (address.Quality < 8)
        {
          try
          {
            ModelState.SetModelValue("Zip", new ValueProviderResult(fields["Zip"], fields["Zip"], CultureInfo.CurrentUICulture));
            // This is supposed to be UpdateModel, not TryUpdate
            UpdateModel(address, new[] { "Zip" });
          }
          catch (Exception)
          {
            ModelState.AddModelError("Zip", "Can't locate address. ZIP is required");
          }
        }

        this.db.PersonAddress.Add(address);
      }

      foreach (string contact in new[] { "Home", "Work", "Cell" })
      {
        if (string.IsNullOrEmpty(fields[contact + "Phone"]))
        {
          continue;
        }

        ModelState.SetModelValue(contact + "Phone", new ValueProviderResult(fields[contact + "Phone"], fields[contact + "Phone"], CultureInfo.CurrentUICulture));
        PersonContact pc = new PersonContact { Person = m, Type = "phone", Subtype = contact.ToLower(), Value = fields[contact + "Phone"] };
        this.db.PersonContact.Add(pc);
      }

      if (!string.IsNullOrEmpty(fields["HamCall"]))
      {
        ModelState.SetModelValue("HamCall", new ValueProviderResult(fields["HamCall"], fields["HamCall"], CultureInfo.CurrentUICulture));
        PersonContact pc = new PersonContact { Person = m, Type = "hamcall", Value = fields["HamCall"] };
        this.db.PersonContact.Add(pc);
      }

      if (!string.IsNullOrEmpty(fields["Email"]))
      {
        ModelState.SetModelValue("Email", new ValueProviderResult(fields["Email"], fields["Email"], CultureInfo.CurrentUICulture));
        PersonContact pc = new PersonContact { Person = m, Type = "email", Value = fields["Email"] };
        this.db.PersonContact.Add(pc);
      }

      if (!string.IsNullOrEmpty(fields["Email2"]))
      {
        ModelState.SetModelValue("Email2", new ValueProviderResult(fields["Email2"], fields["Email2"], CultureInfo.CurrentUICulture));
        PersonContact pc = new PersonContact { Person = m, Type = "email", Value = fields["Email2"] };
        this.db.PersonContact.Add(pc);
      }

      DateTime courseDate = new DateTime(1900, 1, 1);
      ModelState.SetModelValue("CourseDate", new ValueProviderResult(fields["CourseDate"], fields["CourseDate"], CultureInfo.CurrentUICulture));
      if (string.IsNullOrEmpty(fields["CourseDate"]))
      {
        ModelState.AddModelError("CourseDate", "Required");
        return null;
      }
      else if (!DateTime.TryParse(fields["CourseDate"], out courseDate))
      {
        ModelState.AddModelError("CourseDate", "Unknown format. Try yyyy-mm-dd");
        return null;
      }
      courseDate = courseDate.Date;

      UnitMembership um = new UnitMembership { Person = m, Status = status, Unit = esar, Activated = courseDate };
      this.db.UnitMemberships.Add(um);

      TrainingCourse courseA = (from tc in this.db.TrainingCourses where tc.DisplayName == "Course A" select tc).First();
      DateTime nextDate = courseDate.AddDays(1);

      Training t = (from trn in this.db.Trainings where trn.StartTime >= courseDate && trn.StartTime < nextDate && trn.Title == "Course A" select trn).FirstOrDefault();
      if (t == null)
      {
        t = new Training();
        t.OfferedCourses.Add(courseA);
        t.StartTime = courseDate.AddHours(19);
        t.StopTime = courseDate.AddHours(21);
        t.County = "King";
        t.Title = "Course A";
        t.Location = "Eastside Fire Headquarters";
        this.db.Trainings.Add(t);
      }

      TrainingRoster tr = new TrainingRoster { Person = m, TimeIn = courseDate.AddHours(18), TimeOut = courseDate.AddHours(22) };
      this.db.TrainingRosters.Add(tr);
      t.Roster.Add(tr);

      TrainingAward ta = new TrainingAward();
      ta.Completed = courseDate.AddHours(21);
      if ((courseA.ValidMonths ?? 0) > 0)
      {
        ta.Expiry = ta.Completed.AddMonths(courseA.ValidMonths.Value);
      }
      ta.Course = courseA;
      ta.Member = m;
      this.db.TrainingAward.Add(ta);
      tr.TrainingAwards.Add(ta);
      return m;
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Delete(Guid id)
    {
      return View(GetMember(id));
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult Delete(Guid id, FormCollection fields)
    {
      Member m = GetMember(id);
      this.db.Members.Remove(m);
      this.db.SaveChanges();

      return RedirectToAction("Index");
    }

    [Authorize]
    public ActionResult CardDatabaseDifferences(bool? commit, Guid[] except)
    {
      var cardMembers = CardDatabaseService.GetEmergencyWorkers();
      var cardMembersDict = cardMembers.ToDictionary(f => f.PK, f => f);

      var dbMembers = this.db.Members
          .Where(f => f.WacLevel > WacLevel.None && f.Memberships.Any(g => g.EndTime == null && g.Status.IsActive) && f.ExternalKey1 != null)
          .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();

      StringBuilder builder = new StringBuilder();

      var d = 0;

      while (d < dbMembers.Count)
      {
        if (cardMembersDict.ContainsKey(dbMembers[d].ExternalKey1.Value))
        {
          var cardMember = cardMembersDict[dbMembers[d].ExternalKey1.Value];

          string matchText = "";
          if (cardMember.IdentityCode != dbMembers[d].DEM)
            matchText += string.Format(", KCSO DEM={0} KCSARA DEM={1}", cardMember.IdentityCode, dbMembers[d].DEM);
          if (cardMember.Rank != dbMembers[d].WacLevel.ToString())
            matchText += string.Format(", KCSO Card={0} KCSARA Card={1}", cardMember.Rank, dbMembers[d].WacLevel.ToString());
          if (matchText.Length > 0)
            builder.AppendFormat("<b>{0}</b> has differences{1}<br/>", dbMembers[d].ReverseName, matchText);

          if (!cardMember.LastName.Equals(dbMembers[d].LastName, StringComparison.OrdinalIgnoreCase) || !cardMember.FirstName.Equals(dbMembers[d].FirstName, StringComparison.OrdinalIgnoreCase))
            builder.AppendFormat("<span style=\"color:#cccccc\">KCSARA member {0} in KCSO database as {1}, {2}</span><br/>", dbMembers[d].ReverseName, cardMember.LastName, cardMember.FirstName);


          cardMembersDict.Remove(dbMembers[d].ExternalKey1.Value);
          dbMembers.RemoveAt(d);
        }
        else
        {
          builder.AppendFormat("<b><a href=\"{1}\">{0}</a></b> was linked to a card database record that no longer exists<br/>", dbMembers[d].ReverseName, Url.Action("Detail", new { id = dbMembers[d].Id }));
          d++;
        }
      }

      cardMembers = cardMembersDict.Values.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();
      dbMembers = this.db.Members.Where(f => f.WacLevel > WacLevel.None && f.Memberships.Any(g => g.EndTime == null && g.Status.IsActive) && f.ExternalKey1 == null)
          .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();

      d = 0;
      while (d < dbMembers.Count)
      {
        var dbMember = dbMembers[d];

        var matches = cardMembers.Where(f => f.LastName.Equals(dbMember.LastName, StringComparison.OrdinalIgnoreCase) && f.FirstName.Equals(dbMember.FirstName, StringComparison.OrdinalIgnoreCase));
        if (matches.Count() > 1)
        {
          matches = cardMembers.Where(f => f.IdentityCode == dbMember.DEM && f.LastName.Equals(dbMember.LastName, StringComparison.OrdinalIgnoreCase) && f.FirstName.Equals(dbMember.FirstName, StringComparison.OrdinalIgnoreCase));
        }
        else if (matches.Count() == 0)
        {
          matches = cardMembers.Where(f => f.IdentityCode == dbMember.DEM && f.LastName.Equals(dbMember.LastName, StringComparison.OrdinalIgnoreCase));
        }


        if (matches.Count() == 0)
        {
          builder.AppendFormat("Can't find <b><a href=\"{1}\">{0}</a></b> in card database<br/>", dbMember.ReverseName, Url.Action("Detail", new { id = dbMember.Id }));
          d++;
        }
        else if (matches.Count() == 1)
        {
          var cardMember = matches.First();

          builder.AppendFormat("New linked member: <b>{0}, {1}</b> ({2}) - {3}  <i><a href=\"{5}\">{4}</a> [{6}]</i><br/>", cardMember.LastName, cardMember.FirstName, cardMember.IdentityCode, cardMember.Rank, dbMember.ReverseName, Url.Action("Detail", new { id = dbMember.Id }), cardMember.PK);

          string matchText = "";
          if (cardMember.IdentityCode != dbMember.DEM)
            matchText += string.Format(", KCSO DEM={0} KCSARA DEM={1}", cardMember.IdentityCode, dbMember.DEM);
          if (cardMember.Rank != dbMember.WacLevel.ToString())
            matchText += string.Format(", KCSO Card={0} KCSARA Card={1}", cardMember.Rank, dbMember.WacLevel.ToString());

          if (matchText.Length > 0)
            builder.AppendFormat("<b>{0}</b> has differences{1}<br/>", dbMember.ReverseName, matchText);

          if (except == null || !except.Contains(dbMember.Id))
          {
            dbMember.ExternalKey1 = cardMember.PK;
          }
          else
          {
            builder.AppendFormat("Skipped linking by request<br/>");
          }

          cardMembers.Remove(cardMember);
          dbMembers.RemoveAt(d);
        }
        else
        {
          builder.AppendFormat("Multiple card records with name {0} and DEM {1}<br/>", dbMember.FullName, dbMember.DEM);
          dbMembers.RemoveAt(d);
          cardMembers.RemoveAll(f => f.LastName.Equals(dbMember.LastName, StringComparison.OrdinalIgnoreCase) &&
              f.FirstName.Equals(dbMember.FirstName, StringComparison.OrdinalIgnoreCase) &&
              f.IdentityCode == dbMember.DEM);
        }
      }

      foreach (var cardMember in cardMembers)
      {
        var matches = this.db.Members.Where(f => f.ExternalKey1 == cardMember.PK);

        if (matches.Count() < 1)
        {
          matches = this.db.Members.Where(f => f.LastName == cardMember.LastName && f.FirstName == cardMember.FirstName);
        }

        if (matches.Count() > 1)
        {
          matches = this.db.Members.Where(f => f.DEM == cardMember.IdentityCode && f.LastName == cardMember.LastName && f.FirstName == cardMember.FirstName);
        }
        else if (matches.Count() == 0)
        {
          matches = this.db.Members.Where(f => f.DEM == cardMember.IdentityCode && f.LastName == cardMember.LastName);
        }

        if (matches.Count() == 0)
        {
          builder.AppendFormat("{0}, {1} ({2}) not found in KCSARA database. [card id {3}]<br/>", cardMember.LastName, cardMember.FirstName, cardMember.IdentityCode, cardMember.PK);
          if (cardMember.IdentityCode.Contains("D"))
          {
            builder.AppendFormat("Should change Rank for {0}, {1} to 'Canine'<br/>", cardMember.LastName, cardMember.FirstName);
          }
        }
        else if (matches.Count() == 1)
        {
          builder.AppendFormat("<a href=\"{2}\">{0}, {1}</a> is no longer mission active with KCSARA<br/>", cardMember.LastName, cardMember.FirstName, Url.Action("Detail", new { id = matches.First().Id }));
        }
        else
        {
          builder.AppendFormat("{0}, {1} has multiple matches in the KCSARA database.<br/>", cardMember.LastName, cardMember.FirstName);
        }

      }

      if (commit ?? false) this.db.SaveChanges();

      return Content(builder.ToString());
    }

    [Authorize(Roles = "cdb.badges")]
    public ActionResult Badges(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        throw new ApplicationException("No ids specified");
      }

      List<Guid> guids = new List<Guid>();
      foreach (string s in id.Split(','))
      {
        guids.Add(new Guid(s));
      }

      var x = (from m in this.db.Members/*.Include("Memberships.Unit").Include("ComputedAwards").Include("ComputedAwards.Course").Include("Memberships.Status")*/ select m).WhereIn(c => c.Id, guids).OrderBy(o => o.LastName + "," + o.FirstName);

      var memStream = BadgeService.CreateCards(x, DateTime.Today.AddYears(3));
      return new FileStreamResult(memStream, "application/pdf") { FileDownloadName = "badges.pdf" };
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize]
    public ActionResult Create()
    {
      if (!Permissions.IsAdmin) return this.CreateLoginRedirect();

      ViewData["PageTitle"] = "New Member";

      Member m = new Member();
      m.WacLevel = WacLevel.None;
      Session["NewUserGuid"] = Guid.NewGuid();
      ViewData["NewUserGuid"] = Session["NewUserGuid"];
      return InternalEdit(m);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult Create(FormCollection fields)
    {
      if (!Permissions.IsAdmin) return this.CreateLoginRedirect();

      if (Session["NewUserGuid"] != null && Session["NewUserGuid"].ToString() != fields["NewUserGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create a user?");
      }
      Session.Remove("NewUserGuid");

      ViewData["PageTitle"] = "New Member";

      Member m = new Member();
      this.db.Members.Add(m);
      return InternalSave(m, fields, RedirectToAction("Detail", new { id = m.Id }));
    }

    [AcceptVerbs("GET")]
    [Authorize]
    public ActionResult Edit(Guid id)
    {
      if (!(Permissions.IsAdmin || Permissions.IsMembershipForPerson(id))) return this.CreateLoginRedirect();


      Member m = GetMember(id);
      return InternalEdit(m);
    }

    private ActionResult InternalEdit(Member m)
    {
      ViewData["AdminEdit"] = Permissions.IsAdmin;

      ViewData["Gender"] = new SelectList(Enum.GetNames(typeof(Gender)), m.Gender.ToString());
      ViewData["WacLevel"] = new SelectList(Enum.GetNames(typeof(WacLevel)), m.WacLevel.ToString());

      return View("Edit", m);
    }

    [AcceptVerbs("POST")]
    [Authorize]
    public ActionResult Edit(Guid id, FormCollection fields)
    {
      if (!(Permissions.IsAdmin || Permissions.IsMembershipForPerson(id))) return this.CreateLoginRedirect();

      return InternalSave(GetMember(id), fields, RedirectToAction("ClosePopup"));
    }

    private ActionResult InternalSave(Member m, FormCollection fields, ActionResult successAction)
    {
      if (Permissions.IsAdmin)
      {
        TryUpdateModel(m, new string[] { "DEM", "WacLevel", "WacLevelDate", "BackgroundDate", "SheriffApp" });

        // When creating a new user, the above methods will set ModelState to Invalid
        // and the call below doesn't clear it. Reset it now and the method
        // below will flag it Invalid if neededd.
        ModelState.Remove("FirstName");
      }

      if (Permissions.IsAdmin || Permissions.IsMembershipForPerson(m.Id))
      {
        TryUpdateModel(m, new string[] { "FirstName", "LastName", "MiddleName", "Gender", "BirthDate", "ExternalKey1" });
      }

      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        TempData["message"] = "Saved";
        return successAction;
      }

      return InternalEdit(m);
    }

    #region Photos
    [Authorize]
    public ActionResult PhotoUpload(string id)
    {
      string[] split = id.Split(',');
      List<Guid> ids = new List<Guid>();
      foreach (string s in split)
      {
        try
        {
          Guid g = new Guid(s);
          if (Permissions.IsAdmin || Permissions.IsMembershipForPerson(g) || Permissions.IsInRole(new[] { "cdb.photos" }))
          {
            ids.Add(g);
          }
        }
        finally { }
      }

      if (ids.Count == 0 && !Permissions.IsAdmin) return this.CreateLoginRedirect();

      var x = this.db.Members.Where(GetSelectorPredicate<Member>(ids)).OrderBy(f => f.LastName + "," + f.FirstName);

      return View(x);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult PhotoPreview()
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      Dictionary<Guid, Bitmap> images = new Dictionary<Guid, Bitmap>();

      ClearPreviewCache();
      foreach (string file in Request.Files)
      {
        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
          continue;

        Guid id = new Guid(file.Substring(1));

        var imgSvc = new ImageService();
        Bitmap bmPhoto = imgSvc.GetResized(hpf.InputStream, 375, 500);
        images.Add(id, bmPhoto);
      }

      if (images.Count > 0)
      {
        Session["photoPreview"] = images;
      }

      var m = this.db.Members.Where(GetSelectorPredicate<Member>(images.Keys)).OrderBy(f => f.LastName + "," + f.FirstName);

      return View(m);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult PhotoCommit(FormCollection fields)
    {
      List<string> errors = new List<string>();

      if (Session["photoPreview"] == null)
      {
        errors.Add("Temporary data not found.");
      }
      else
      {
        Dictionary<Guid, Bitmap> images = (Dictionary<Guid, Bitmap>)Session["photoPreview"];

        var members = this.db.Members.Where(GetSelectorPredicate<Member>(images.Keys)).OrderBy(f => f.LastName + "," + f.FirstName);

        string keepImages = (fields["keep"] ?? "").ToLowerInvariant();

        char[] badChars = IO.Path.GetInvalidFileNameChars();

        foreach (Member m in members)
        {
          // Permissions check
          if (!(Permissions.IsAdmin || Permissions.IsInRole(new[] { "cdb.photos" }) || Permissions.IsMembershipForPerson(m.Id))) return this.CreateLoginRedirect();

          string storePath = Server.MapPath(api.MembersController.PhotosStoreRelativePath);

          if (keepImages.Contains(m.Id.ToString().ToLowerInvariant()))
          {
            string newFileName = string.Format("{0}{1}.jpg", string.Join("", m.LastName.Split(badChars)), Guid.NewGuid());
            images[m.Id].Save(IO.Path.Combine(storePath, newFileName), ImageFormat.Jpeg);
            m.PhotoFile = newFileName;
          }
        }

        this.db.SaveChanges();
      }


      if (errors.Count > 0)
      {
        ViewData["error"] = string.Join("<br/>", errors.ToArray());
        return View("Error");
      }

      return RedirectToAction("ClosePopup");
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize]
    public StreamResult PhotoData(Guid id)
    {
      if (Session["photoPreview"] == null)
      {
        Response.StatusCode = 400;
        Response.End();
      }

      Dictionary<Guid, Bitmap> photos = (Dictionary<Guid, Bitmap>)Session["photoPreview"];

      StreamResult result = new StreamResult { ContentType = "image/jpeg" };
      photos[id].Save(result.Stream, ImageFormat.Jpeg);
      result.Stream.Position = 0;

      return result;
    }

    private void ClearPreviewCache()
    {
      if (Session["photoPreview"] == null)
      {
        return;
      }

      Dictionary<Guid, Bitmap> photos = (Dictionary<Guid, Bitmap>)Session["photoPreview"];
      foreach (Bitmap b in photos.Values)
      {
        b.Dispose();
      }
      Session["photoPreview"] = null;
    }
    #endregion

    [Authorize]
    public ActionResult Suggest(string q)
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      string query = q;

      IEnumerable<Member> results = null;
      results = (from m in this.db.Members
                 where (m.FirstName + " " + m.LastName).ToLower().Contains(query)
                   || (m.LastName + ", " + m.FirstName).ToLower().Contains(query)
                   || m.DEM.Contains(query)
                 select m);
      return Data(results.Select(f => new ApiModels.MemberSummary(f)).ToArray());
    }


    [Authorize]
    public ActionResult SuggestDem()
    {
      int suggestion = ((DateTime.Now.Year) % 10) * 1000 + 1;

      var x = (from m in this.db.Members orderby m.DEM.Length, m.DEM select m.DEM);
      List<int> demNumbers = new List<int>();

      foreach (string dem in x)
      {
        int result;
        if (!int.TryParse(dem, out result))
        {
          continue;
        }
        if (result >= suggestion)
        {
          demNumbers.Add(result);
        }
      }

      while (demNumbers.Contains(suggestion))
      {
        suggestion++;
      }

      return new ContentResult { Content = suggestion.ToString().PadLeft(4, '0') };
    }

    [Authorize]
    public ActionResult GetMembersActiveUnits(Guid id)
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      Member m = GetMember(this.db.Members.Include("Memberships").Include("Memberships.Status").Include("Memberships.Unit"), id);
      UnitMembership[] ums = m.GetActiveUnits();

      return new JsonResult { Data = ums.Select(f => new { Id = f.Unit.Id }).ToArray(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }

    [Authorize]
    public ActionResult GetMembersAnimals(Guid id)
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      DateTime atTime = DateTime.Now;

      AnimalView[] animals = (from ao in this.db.AnimalOwners.Include("Animal").Include("Owner")
                              where ao.Owner.Id == id && (ao.Ending == null || ao.Ending > atTime)
                              select new AnimalView
                                {
                                  Id = ao.Animal.Id,
                                  Type = ao.Animal.Type,
                                  Name = ao.Animal.Name,
                                  OwnerId = ao.Owner.Id
                                }).ToArray();

      return Data(animals); // new JsonResult { Data = animals, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize]
    public ActionResult CardData()
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      return new ContentResult { Content = "Ready >", ContentType = "text/plain" };
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult CardData(FormCollection columns)
    {
      if (!Permissions.IsUser) return this.CreateLoginRedirect();

      var members = (from m in this.db.Members.Include("Memberships.Unit").Include("Memberships.Status")
          .Include("ComputedAwards").Include("ComputedAwards.Course").Include("ComputedAwards.Rule")
                     select m).OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      MD5 md5 = new MD5CryptoServiceProvider();
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
      sb.AppendLine("<CardData>");
      sb.AppendLine("<Version>1</Version>");
      sb.AppendLine("<Data>");
      foreach (Member member in members)
      {
        UnitMembership[] units = member.GetActiveUnits();
        if (units.Length == 0)
        {
          continue;
        }

        sb.Append(string.Format("<Member First=\"{0}\" Last=\"{1}\" DEM=\"{2}\" Type=\"{3}\"", member.FirstName.ToXmlAttr(), member.LastName.ToXmlAttr(), member.DEM.ToXmlAttr(), member.WacLevel));

        if (!string.IsNullOrEmpty(member.PhotoFile))
        {
          string virtualPathPhoto = api.MembersController.GetPhotoOrFillInPath(member.PhotoFile);
          string photoFile = Server.MapPath(virtualPathPhoto);
          if (IO.File.Exists(photoFile))
          {
            byte[] photoData = IO.File.ReadAllBytes(photoFile);

            string photoUrl = new Uri(Request.Url, VirtualPathUtility.ToAbsolute(virtualPathPhoto)).AbsoluteUri;
            sb.AppendFormat(" Photo=\"{0}\" PhotoMd5=\"{1}\"", photoUrl, BitConverter.ToString(md5.ComputeHash(photoData)).Replace("-", ""));
          }
        }

        foreach (string column in columns.Keys)
        {
          string[] defn = columns[column].Split(':');
          if (defn.Length < 2)
          {
            continue;
          }

          Guid id = new Guid(defn[1]);
          if (defn[0].ToLowerInvariant().EndsWith("course"))
          {
            if (member.ComputedAwards.Where(f => ((f.Course.Id == id) && (f.Expiry == null || f.Expiry > DateTime.Now))).Count() > 0)
            {
              sb.AppendFormat(" {0}=\"1\"", column);
            }
          }
          else if (defn[0].ToLowerInvariant().EndsWith("unit"))
          {
            if (units.Where(f => f.Unit.Id == id).Count() > 0)
            {
              sb.AppendFormat(" {0}=\"1\"", column);
            }
          }
        }

        sb.AppendLine("/>");
      }

      var links = (from ol in this.db.AnimalOwners.Include("Animal").Include("Owner").Include("Owner.Memberships.Unit").Include("Owner.Memberships.Status") where ol.IsPrimary && ol.Ending == null orderby ol.Animal.Id select ol);
      Guid lastAnimalId = Guid.Empty;
      foreach (AnimalOwner ownerLink in links)
      {
        Animal animal = ownerLink.Animal;
        if (animal.Id == lastAnimalId)
        {
          continue;
        }
        lastAnimalId = animal.Id;

        Member owner = ownerLink.Owner;
        UnitMembership[] units = owner.GetActiveUnits();
        if (units.Length == 0)
        {
          continue;
        }

        sb.Append(string.Format("<Animal First=\"{0}\" Last=\"{1}\" DEM=\"{2}\" Type=\"{3}\"", animal.Name, owner.LastName, animal.DemSuffix, WacLevel.Field));

        //if (!string.IsNullOrEmpty(animal.PhotoFile))
        //{
        //    string virtualPathPhoto = AnimalsController.GetPhotoOrFillInPath(animal.PhotoFile);
        //    string photoFile = Server.MapPath(virtualPathPhoto);
        //    if (IO.File.Exists(photoFile))
        //    {
        //        byte[] photoData = IO.File.ReadAllBytes(photoFile);

        //        string photoUrl = new Uri(Request.Url, VirtualPathUtility.ToAbsolute(virtualPathPhoto)).AbsoluteUri;
        //        sb.AppendFormat(" Photo=\"{0}\" PhotoMd5=\"{1}\"", photoUrl, BitConverter.ToString(md5.ComputeHash(photoData)).Replace("-", ""));
        //    }
        //}

        foreach (string column in columns.Keys)
        {
          string[] defn = columns[column].Split(':');
          if (defn.Length < 2)
          {
            continue;
          }

          Guid id = new Guid(defn[1]);
          if (defn[0].ToLowerInvariant().EndsWith("unit"))
          {
            if (units.Where(f => f.Unit.Id == id).Count() > 0)
            {
              sb.AppendFormat(" {0}=\"1\"", column);
            }
          }
        }

        sb.AppendLine("/>");
      }
      sb.AppendLine("</Data>");
      sb.AppendLine("</CardData>");
      return new ContentResult { ContentType = "text/xml", Content = sb.ToString() };
    }


    public static IEnumerable<Member> FilterPersonal(IEnumerable<Member> members)
    {
      foreach (Member m in members)
      {
        FilterPersonal(m);
      }
      return members;
    }

    public static Member FilterPersonal(Member member)
    {
      member.BirthDate = null;
      return member;
    }

    private Member GetMember(Guid id)
    {
      return GetMember(this.db.Members, id);
    }

    private Member GetMember(IEnumerable<Member> context, Guid id)
    {
      List<Member> members = (from m in context where m.Id == id select m).ToList();
      if (members.Count != 1)
      {
        throw new ApplicationException(string.Format("{0} members found with ID = {1}", members.Count, id.ToString()));
      }

      return members[0];
    }

    [Authorize]
    public DataActionResult GetMemberData(Guid id)
    {
      Member m = GetMember(id);
      if (!Permissions.IsAdmin) { FilterPersonal(m); }
      return Data(m);
    }

    #region Unit Memberships
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateMembership(Guid personId)
    {
      ViewData["PageTitle"] = "New Unit Membership";

      UnitMembership s = new UnitMembership();
      s.Person = (from p in this.db.Members where p.Id == personId select p).First();
      s.Activated = DateTime.Today;

      Session.Add("NewMembershipGuid", s.Id);
      ViewData["NewMembershipGuid"] = Session["NewMembershipGuid"];

      return InternalEditMembership(s);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateMembership(Guid personId, FormCollection fields)
    {
      if (Session["NewMembershipGuid"] != null && Session["NewMembershipGuid"].ToString() != fields["NewMembershipGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create a status change?");
      }
      Session.Remove("NewMembershipGuid");

      ViewData["PageTitle"] = "New Unit Membership";

      UnitMembership um = new UnitMembership();
      um.Person = (from p in this.db.Members where p.Id == personId select p).First();
      this.db.UnitMemberships.Add(um);
      return InternalSaveMembership(um, fields);
    }


    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditMembership(Guid id)
    {
      UnitMembership m = GetUnitMembership(id);
      ViewData["showEditWarning"] = true;
      return InternalEditMembership(m);
    }

    private ActionResult InternalEditMembership(UnitMembership um)
    {
      SarUnit[] units = (from u in this.db.Units orderby u.DisplayName select u).ToArray();

      Guid selectedUnit = (um.Unit != null) ? um.Unit.Id : Guid.Empty;

      // MVC RC BUG - Have to store list in a unique key in order for SelectedItem to work
      ViewData["Unit"] = new SelectList(units, "Id", "DisplayName", selectedUnit);

      if (selectedUnit == Guid.Empty && units.Length > 0)
      {
        selectedUnit = units.First().Id;
      }

      ViewData["Status"] = new SelectList(
              (from s in this.db.UnitStatusTypes.Include("Unit") where s.Unit.Id == selectedUnit orderby s.StatusName select s).ToArray(),
              "Id",
              "StatusName",
              (um.Status != null) ? (Guid?)um.Status.Id : null);

      return View("EditMembership", um);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditMembership(Guid id, FormCollection fields)
    {
      UnitMembership um = GetUnitMembership(id);
      return InternalSaveMembership(um, fields);
    }


    private ActionResult InternalSaveMembership(UnitMembership um, FormCollection fields)
    {
      TryUpdateModel(um, new string[] { "Activated", "Comments" });

      Guid unitId = new Guid(fields["Unit"]);
      SarUnit unit = (from u in this.db.Units where u.Id == unitId select u).First();
      um.Unit = unit;

      Guid statusId = new Guid(fields["Status"]);
      UnitStatus status = (from s in this.db.UnitStatusTypes where s.Id == statusId select s).First();
      um.Status = status;

      Guid personId = new Guid(fields["Person"]);
      Member person = (from m in this.db.Members where m.Id == personId select m).First();
      um.Person = person;

      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        TempData["message"] = "Saved";
        UpdateMemberships(um.Person.Id);

        return RedirectToAction("ClosePopup");
      }

      return InternalEditMembership(um);
    }

    private void UpdateMemberships(Guid personId)
    {
      UnitMembership lastUm = null;
      foreach (Kcsar.Database.Model.UnitMembership um in (from u in this.db.UnitMemberships.Include("Person").Include("Unit") where u.Person.Id == personId select u).OrderBy(f => f.Unit.Id).ThenBy(f => f.Activated))
      {
        if (lastUm != null)
        {
          if (um.Unit.Id == lastUm.Unit.Id)
          {
            if (lastUm.EndTime != um.Activated)
            {
              lastUm.EndTime = um.Activated;
            }
          }
          else if (lastUm.EndTime != null)
          {
            lastUm.EndTime = null;
          }
        }
        lastUm = um;
      }
      if (lastUm != null && lastUm.EndTime != null)
      {
        lastUm.EndTime = null;
      }

      this.db.SaveChanges();
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteMembership(Guid id)
    {
      return View(GetUnitMembership(id));
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteMembership(Guid id, FormCollection fields)
    {
      UnitMembership um = GetUnitMembership(id);
      Guid personId = um.Person.Id;

      this.db.UnitMemberships.Remove(um);
      this.db.SaveChanges();

      UpdateMemberships(personId);
      return RedirectToAction("ClosePopup");
    }

    private UnitMembership GetUnitMembership(Guid id)
    {
      return GetUnitMembership(this.db.UnitMemberships.Include("Person").Include("Unit").Include("Status"), id);
    }

    private UnitMembership GetUnitMembership(IEnumerable<UnitMembership> context, Guid id)
    {
      List<UnitMembership> memberships = (from m in context where m.Id == id select m).ToList();
      if (memberships.Count != 1)
      {
        throw new ApplicationException(string.Format("{0} memberships found with ID = {1}", memberships.Count, id.ToString()));
      }

      UnitMembership membership = memberships[0];
      return membership;
    }

    #endregion

    #region Addresses
    [AcceptVerbs("GET")]
    [Authorize]
    public ActionResult CreateAddress(Guid personId)
    {
      if (!Permissions.IsAdmin && !Permissions.IsSelf(personId)) return this.CreateLoginRedirect();

      ViewData["PageTitle"] = "New Address";

      PersonAddress address = new PersonAddress { State = "WA" };
      address.Person = (from p in this.db.Members where p.Id == personId select p).First();

      Session.Add("NewAddressGuid", address.Id);
      ViewData["NewAddressGuid"] = Session["NewAddressGuid"];

      return InternalEditAddress(address);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult CreateAddress(Guid personId, FormCollection fields)
    {
      if (!Permissions.IsAdmin && !Permissions.IsSelf(personId)) return this.CreateLoginRedirect();

      if (Session["NewAddressGuid"] != null && Session["NewAddressGuid"].ToString() != fields["NewAddressGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create a status change?");
      }
      Session.Remove("NewAddressGuid");

      ViewData["PageTitle"] = "New Address";

      PersonAddress address = new PersonAddress();
      address.Person = (from p in this.db.Members where p.Id == personId select p).First();
      this.db.PersonAddress.Add(address);
      return InternalSaveAddress(address, fields);
    }


    [AcceptVerbs("GET")]
    [Authorize]
    public ActionResult EditAddress(Guid id)
    {
      PersonAddress address = (from a in this.db.PersonAddress.Include("Person") where a.Id == id select a).First();

      if (!Permissions.IsAdmin && !Permissions.IsSelf(address.Person.Id)) return this.CreateLoginRedirect();

      return InternalEditAddress(address);
    }

    private ActionResult InternalEditAddress(PersonAddress address)
    {
      ViewData["State"] = new SelectList(new string[] { "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY" }, address.State.ToUpperInvariant());

      ViewData["Type"] = new SelectList(Enum.GetNames(typeof(PersonAddressType)), address.Type.ToString());

      return View("EditAddress", address);
    }

    [AcceptVerbs("POST")]
    [Authorize]
    public ActionResult EditAddress(Guid id, FormCollection fields)
    {
      PersonAddress address = (from a in this.db.PersonAddress.Include("Person") where a.Id == id select a).First();

      if (!Permissions.IsAdmin && !Permissions.IsSelf(address.Person.Id)) return this.CreateLoginRedirect();

      return InternalSaveAddress(address, fields);
    }


    private ActionResult InternalSaveAddress(PersonAddress address, FormCollection fields)
    {
      TryUpdateModel(address, new string[] { "Street", "City", "State", "Zip", "Type" });

      Guid personId = new Guid(fields["Person"]);
      Member person = (from m in this.db.Members where m.Id == personId select m).First();
      address.Person = person;
      address.Quality = 0;
      address.Location = null;

      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        TempData["message"] = "Saved";
        return RedirectToAction("ClosePopup");
      }
      return InternalEditAddress(address);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize]
    public ActionResult DeleteAddress(Guid id)
    {
      ViewData["HideFrame"] = true;
      PersonAddress address = (from a in this.db.PersonAddress.Include("Person") where a.Id == id select a).First();

      if (!Permissions.IsAdmin && !Permissions.IsSelf(address.Person.Id)) return this.CreateLoginRedirect();

      return View(address);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize]
    public ActionResult DeleteAddress(Guid id, FormCollection fields)
    {
      PersonAddress address = (from a in this.db.PersonAddress.Include("Person") where a.Id == id select a).First();

      if (!Permissions.IsAdmin && !Permissions.IsSelf(address.Person.Id)) return this.CreateLoginRedirect();

      this.db.PersonAddress.Remove(address);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }
    #endregion

    #region Contact Info
    public DataActionResult GetContacts(Guid id)
    {
      if (!(Permissions.IsUser || Permissions.IsSelf(id))) return GetLoginError();

      List<MemberContactView> model = null;

      model = (from c in this.db.PersonContact
               where c.Person.Id == id
               select new MemberContactView
               {
                 Id = c.Id,
                 MemberId = c.Person.Id,
                 Type = c.Type,
                 SubType = c.Subtype,
                 Value = c.Value,
                 Priority = c.Priority
               }).OrderBy(f => f.Type).ThenBy(f => f.Priority).ThenBy(f => f.SubType).ToList();

      return Data(model);
    }

    public DataActionResult PromoteContact(Guid id)
    {
      List<SubmitError> errors = new List<SubmitError>();
      var promotee = (from c in this.db.PersonContact.Include("Person") where c.Id == id select c).FirstOrDefault();
      // TODO: Error if the contact isn't found.

      if (!Permissions.IsAdmin && !Permissions.IsSelf(promotee.Person.Id) && !Permissions.IsMembershipForPerson(promotee.Person.Id)) return GetLoginError();

      promotee.Priority = 0;
      foreach (var contact in (from c in this.db.PersonContact where c.Person.Id == promotee.Person.Id && c.Type == promotee.Type && c.Priority == 0 select c))
      {
        contact.Priority = 1;
      }
      try
      {
        this.db.SaveChanges();
      }
      catch (DbEntityValidationException ex)
      {
        foreach (var entry in ex.EntityValidationErrors.Where(f => !f.IsValid))
        {
          foreach (var err in entry.ValidationErrors)
          {
            errors.Add(new SubmitError { Error = err.ErrorMessage, Property = err.PropertyName, Id = new[] { ((IModelObject)entry.Entry.Entity).Id } });
          }
        }
      }

      return Data(new SubmitResult<bool> { Errors = errors.ToArray(), Result = (errors.Count == 0) });
    }

    public DataActionResult SubmitContact(/*[ModelBinder(typeof(JsonDataContractBinder<MemberContactView>))] */MemberContactView view)
    {
      if (!Permissions.IsAdmin && !Permissions.IsSelf(view.MemberId) && !Permissions.IsMembershipForPerson(view.MemberId)) return GetLoginError();

      List<SubmitError> errors = new List<SubmitError>();

      PersonContact model = (from c in this.db.PersonContact.Include("Person") where c.Id == view.Id select c).FirstOrDefault();
      if (model == null)
      {
        model = new PersonContact();
        model.Priority = 0;
        if ((from c in this.db.PersonContact where c.Person.Id == view.MemberId && c.Type == view.Type select c.Id).Count() > 0)
        {
          model.Priority = 1;
        }
        this.db.PersonContact.Add(model);
      }

      try
      {
        if (model.Type != view.Type) model.Type = view.Type;
        if (model.Subtype != view.SubType) model.Subtype = view.SubType;
        if (model.Value != view.Value) model.Value = view.Value;
        if (model.Person == null || model.Person.Id != view.MemberId) model.Person = (from m in this.db.Members where m.Id == view.MemberId select m).FirstOrDefault();

        this.db.SaveChanges();

        view.Id = model.Id;
        view.Priority = model.Priority;
      }
      catch (DbEntityValidationException ex)
      {
        foreach (var entry in ex.EntityValidationErrors.Where(f => !f.IsValid))
        {
          foreach (var err in entry.ValidationErrors)
          {
            errors.Add(new SubmitError { Error = err.ErrorMessage, Property = err.PropertyName, Id = new[] { ((IModelObject)entry.Entry.Entity).Id } });
          }
        }
      }

      return Data(new SubmitResult<MemberContactView>
      {
        Errors = errors.ToArray(),
        Result = (errors.Count > 0) ?
            (MemberContactView)null :
            view
      });
    }

    public DataActionResult DeleteContact(Guid id)
    {
      if (!Permissions.IsAuthenticated)
      {
        return GetLoginError();
      }

      var match = (from c in this.db.PersonContact where c.Id == id select new { Id = c.Person.Id, Type = c.Type }).First();

      var contacts = (from c in this.db.PersonContact.Include("Person") where c.Person.Id == match.Id && c.Type == match.Type select c).ToList();

      if (!Permissions.IsAdmin && !Permissions.IsSelf(contacts[0].Person.Id) && !Permissions.IsMembershipForPerson(contacts[0].Person.Id)) return GetLoginError();

      // Get a hold of the contact to delete.
      var contact = contacts.Where(f => f.Id == id).Single();

      // If this is the primary contact for the type group, pick another (if one exists) to be the primary.
      if (contact.Priority == 0)
      {
        var replacement = contacts.Where(f => f.Id != id).FirstOrDefault();
        if (replacement != null)
        {
          replacement.Priority = 0;
        }
      }

      // Delete the specified contact.
      this.db.PersonContact.Remove(contact);
      this.db.SaveChanges();

      return Data(new SubmitResult<bool> { Result = true, Errors = new SubmitError[0] });
    }

    [HttpPost]
    public DataActionResult GetContactInfoSubTypes(string type)
    {
      return Data(PersonContact.GetSubTypes(type));
    }

    [HttpPost]
    public ActionResult GetGeographies(Guid? unitId)
    {
      if (!User.IsInRole("cdb.users")) return GetLoginError();

      MapDataView model = new MapDataView();
      //var members = this.db.Members
      //    .Where(f => f.Memberships.Any(g => (g.Unit.Id == unitId || unitId == null) && g.Status.IsActive && g.EndTime == null))
      //    .ToDictionary(f => f.Id, f => f);


      var geographies = this.db.Members
          .Where(f => f.Memberships.Any(g => (g.Unit.Id == unitId || unitId == null) && g.Status.IsActive && g.EndTime == null))
          .SelectMany(f => f.Addresses).Where(f => f.Geo != null).OrderBy(f => f.Geo);

      Guid lastPerson = Guid.Empty;
      string lastGeo = string.Empty;
      GeographyView view = null;

      foreach (var row in geographies)
      {
        //Guid personRef = (Guid)row.PersonReference.EntityKey.EntityKeyValues.Single().Value;
        //if (personRef == lastPerson || !members.ContainsKey(personRef))
        if (row.Person.Id == lastPerson)
        {
          continue;
        }
        //lastPerson = personRef;
        lastPerson = row.Person.Id;

        if (row.Geo == lastGeo)
        {
          view.Description = PersonDescription(row.Person, this.db) + view.Description;
          continue;
        }
        lastGeo = row.Geo;

        view = GeographyView.BuildGeographyView(row);
        string[] parts = view.Description.Split('\n');
        view.Description = PersonDescription(row.Person, this.db) + string.Join("<br/>", parts.Skip(1));
        model.Items.Add(view);
      }
      return Data(model);
    }

    private string PersonDescription(Member m, IKcsarContext ctx)
    {
      return string.Format("{0} [{1}] <a href=\"{2}\" target=\"_blank\">Detail</a><br/>",
                      m.FullName,
                      string.Join("][", m.Memberships.Where(ctx.GetActiveMembershipFilter(null, DateTime.Now)).Select(f => f.Unit.DisplayName).OrderBy(f => f).ToArray()),
                      Url.Action("Detail", new { id = m.Id }));
    }
    #endregion

    [Authorize]
    public ActionResult Promotions()
    {
      return View();
    }

    [Authorize]
    public ActionResult PromotionsResult(List<Guid> m, bool? relative)
    {
      List<string> names;
      List<Dictionary<DateTime, int>> hourSeries = new List<Dictionary<DateTime, int>>();
      List<Dictionary<DateTime, Tuple<int, string>>> promotions = new List<Dictionary<DateTime, Tuple<int, string>>>();
      Guid esar = new Guid("C2F99BB4-3056-4097-9345-4B8797F40E10");

      names = this.db.Members.WhereIn(f => f.Id, m).OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ThenBy(f => f.Id).Select(f => f.FirstName + " " + f.LastName).ToList();

      var courses = (from c in this.db.TrainingCourses where c.Unit.Id == esar && c.Categories.Contains("leadership") select c);

      var responses = this.db.MissionRosters.WhereIn(f => f.Person.Id, m).Where(f => f.InternalRole == "field" || f.InternalRole == "ol").Where(f => f.TimeIn != null && f.TimeOut != null).OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName).ThenBy(f => f.Person.Id).ThenBy(f => f.TimeIn);

      Guid lastId = Guid.Empty;
      Dictionary<DateTime, int> hours = new Dictionary<DateTime, int>();
      Dictionary<DateTime, Tuple<int, string>> promotes = new Dictionary<DateTime, Tuple<int, string>>();
      double hoursSum = 0;
      TimeSpan offset = TimeSpan.FromSeconds(0);
      DateTime lastTime = DateTime.MinValue;
      int recordRow = 0;
      List<TrainingAward> records = null;
      foreach (var row in responses)
      {
        //Guid personId = (Guid)row.PersonReference.EntityKey.EntityKeyValues.First().Value;
        Guid personId = row.Person.Id;
        if (personId != lastId)
        {
          if (lastId != Guid.Empty)
          {
            hourSeries.Add(hours);
            promotions.Add(promotes);
          }

          records = this.db.TrainingAward.Include("Course").WhereIn(f => f.Course.Id, courses.Select(f => f.Id)).Where(f => f.Member.Id == personId).OrderBy(f => f.Completed).ToList();
          recordRow = 0;
          hoursSum = 0;
          lastId = personId;
          hours = new Dictionary<DateTime, int>();
          promotes = new Dictionary<DateTime, Tuple<int, string>>();
          if (relative.HasValue && relative.Value)
          {
            offset = (row.TimeIn ?? row.Mission.StartTime) - new DateTime(1900, 1, 1);
          }
          lastTime = DateTime.MinValue;
        }

        if (recordRow < records.Count && records[recordRow].Completed < row.TimeIn.Value)
        {
          promotes.Add(records[recordRow].Completed - offset, new Tuple<int, string>((int)hoursSum, records[recordRow].Course.DisplayName));
          recordRow++;
        }


        DateTime x = row.TimeIn.Value - offset;
        hoursSum += row.Hours.Value;
        if (x == lastTime)
        {
          hours[x] = (int)hoursSum;
        }
        else
        {
          hours.Add(x, (int)hoursSum);
        }

        lastTime = x;
      }
      if (lastId != Guid.Empty)
      {
        hourSeries.Add(hours);
        promotions.Add(promotes);
      }

      ViewData["titles"] = names;
      ViewData["data"] = hourSeries;
      ViewData["promotes"] = promotions;
      return View();
    }

    [Authorize]
    public ActionResult ReconcileMemberStatus(string id)
    {
      Guid unitId = new Guid("c2f99bb4-3056-4097-9345-4b8797f40e10");
      if (!Guid.TryParse(id, out unitId))
      {
        unitId = new Guid("c2f99bb4-3056-4097-9345-4b8797f40e10");
      }

      string debug = string.Empty;
      DateTime oneYear = DateTime.Today.AddYears(-1);

      var model = (from m in this.db.Members.Include("Memberships.Unit").Include("Memberships.Status") where m.Memberships.Any(f => f.Unit.Id == unitId && (!f.EndTime.HasValue || f.EndTime > oneYear)) && m.WacLevel != WacLevel.None select m)
          .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();

      var courses = (from c in this.db.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToList();

      int changeCount = 0;
      foreach (Member m in model)
      {

        WacLevel newRole = m.WacLevel;
        string reason = string.Empty;
        string status = "error";
        try
        {
          status = m.Memberships.Where(f => f.Unit.Id == unitId && !f.EndTime.HasValue).Select(f => f.Status.StatusName).Single();
        }
        catch (InvalidOperationException)
        {
          status = string.Join(", ", m.Memberships.Where(f => f.Unit.Id == unitId && !f.EndTime.HasValue).Select(f => f.Status.StatusName + f.Activated.ToShortDateString()).ToArray());
        }
        string newStatus = status;

        if (m.Memberships.Any(f => f.Unit.Id == unitId && f.Status.IsActive && f.EndTime == null))
        {
          // Active with this unit

          if (m.WacLevel == WacLevel.Novice && m.WacLevelDate < oneYear)
          {
            Member mbr = this.db.Members.Include("ComputedAwards.Course").First(f => f.Id == m.Id);
            mbr.WacLevel = WacLevel.Field;
            var expires = CompositeTrainingStatus.Compute(mbr, courses, DateTime.Now);
            mbr.WacLevel = newRole;

            if (expires.IsGood)
            {
              newRole = WacLevel.Field;
              reason = "Novice > 1yr, WACs Current<br/>";
            }
            else
            {
              // TODO: move someone to support if they've been on missions?

              if (expires.Expirations.Any(f => (f.Value.Status & ExpirationFlags.Missing) == ExpirationFlags.Missing))
              {
                newRole = WacLevel.None;
                newStatus = "Admin";
              }
              else
              {
                newRole = WacLevel.Support;
                newStatus = "Support";
              }

              reason += "Novice > 1 year, delinquint in " + string.Join(",", expires.Expirations.Where(f => (f.Value.Status & ExpirationFlags.Okay) != ExpirationFlags.Okay).Select(f => courses.Single(g => g.Id == f.Key).DisplayName).ToArray()) + "<br/>";
            }
          }
          else if (m.WacLevel == WacLevel.Field || m.WacLevel == WacLevel.Support)
          {
            Member mbr = this.db.Members.Include("ComputedAwards.Course").First(f => f.Id == m.Id);
            var expires = CompositeTrainingStatus.Compute(mbr, courses, DateTime.Now);

            if (!expires.IsGood)
            {
              int daysLate = 0;
              foreach (var pair in expires.Expirations)
              {
                if ((pair.Value.Status & ExpirationFlags.Okay) != ExpirationFlags.Okay)
                {
                  if (pair.Value.Status == ExpirationFlags.Missing)
                  {
                    daysLate = int.MaxValue;
                  }
                  else if (pair.Value.Status == ExpirationFlags.Expired)
                  {
                    daysLate = (DateTime.Today - pair.Value.Expires.Value).Days;
                  }
                }
              }
              if (daysLate > 365)
              {
                newStatus = "Admin";
                newRole = WacLevel.None;
                reason += "Required Training missing or more than 1 yr expired.<br/>";
              }
            }

            DateTime missionCutoff = DateTime.Today.AddYears(-2);
            int missions = this.db.MissionRosters.Where(f => f.Person.Id == m.Id && f.TimeIn > missionCutoff && f.InternalRole != "Responder").Count();

            if (missions == 0)
            {
              newStatus = "Admin";
              newRole = WacLevel.None;
              reason += "No mission assignments in 2 years.<br/>";
            }

          }
        }
        else
        {
          // Not active with this unit.
          newStatus = "Inactive";

          if (m.Memberships.Any(f => f.Status.IsActive && f.EndTime == null))
          {
            // Active with another unit
            reason = "Not active with this unit, active with other(s)";
          }
          else
          {
            // Not active with any unit
            newRole = WacLevel.None;

            var previousUnits = m.Memberships.Where(f => f.Status.IsActive);
            if (previousUnits.Count() > 0)
            {
              reason = "Not active with any unit (left last on " + previousUnits.Max(f => f.EndTime.Value).ToString("yyyy-MM-dd)<br/>");
            }
            else
            {
              reason = "Never active with any unit<br/>";
            }
          }
        }

        if (newRole != m.WacLevel || newStatus != status)
        {
          string otherUnits = (m.Memberships.Any(f => f.Status.IsActive && !f.EndTime.HasValue && f.Unit.Id != unitId)) ?
              " (Check other units)" : "";

          debug += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}{4}</td><td>{5}</td></tr>\n", m.ReverseName, m.WacLevel, reason, newRole, otherUnits, newStatus);
          changeCount++;
        }
      }


      return new ContentResult { Content = "<table><tr><th>Name</th><th>Current Role</th><th>Reason for Change</th><th>Suggested WAC Role</th><th>Suggested ESAR status</th><tr>" + debug + "</table>" + changeCount.ToString() + " changes", ContentType = "text/html" };
    }


    [HttpGet]
    [Authorize]
    public ActionResult ReconcileEmergencyWorkers()
    {
      if (IO.File.Exists(this.ReconcileEmergencyWorkersCache))
      {
        ViewData["lastFileDate"] = IO.File.GetLastWriteTime(this.ReconcileEmergencyWorkersCache).ToString("yyyy-MM-dd HH:mm");
      }
      return View();
    }

    private string ReconcileEmergencyWorkersCache { get { return Server.MapPath("~/Content/auth/reconcile-dem.xlsx"); } }

    [HttpPost]
    [Authorize]
    public ActionResult ReconcileEmergencyWorkers(bool? saveFile)
    {
      if (Request.Files.Count > 1)
      {
        throw new InvalidOperationException("Can only submit one roster");
      }

      byte[] fileData;

      var postedFile = Request.Files[0];
      string fileName = postedFile.FileName;
      string cachePath = this.ReconcileEmergencyWorkersCache;

      if (!string.IsNullOrWhiteSpace(fileName))
      {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
          throw new InvalidOperationException("Must use XLSX file");
        }

        fileData = new byte[postedFile.ContentLength];
        postedFile.InputStream.Read(fileData, 0, fileData.Length);

        if (saveFile ?? false)
        {
          string[] pieces = postedFile.FileName.Split('.');
          string extension = pieces.Last();

          IO.File.WriteAllBytes(cachePath, fileData);
        }
      }
      else if (IO.File.Exists(cachePath))
      {
        fileData = IO.File.ReadAllBytes(cachePath);
        fileName = IO.Path.GetFileName(cachePath);
      }
      else
      {
        throw new InvalidOperationException("No new file was uploaded, and no older file is cached");
      }

      var stream = new System.IO.MemoryStream(fileData);
      stream.Position = 0;

      ExcelFile xl = ExcelService.Read(stream, ExcelFileType.XLSX);
      ExcelSheet sheet = xl.GetSheet(0);

      string[] columns = new[] { "EMT?", "DEM #", "LastName", "FirstName", "Common Name", "ns1:OrgRank", "ns1:Status" };
      for (int i = 0; i < columns.Length; i++)
      {
        string column = sheet.CellAt(0, i).StringValue;
        if (column != columns[i])
        {
          return new ContentResult { Content = "Unexpected column: " + column };
        }
      }

      ReconcileEmergencyWorkersViewModel model = new ReconcileEmergencyWorkersViewModel();
      List<Guid> seen = new List<Guid>();
      int row = 1;
      DateTime now = DateTime.Now;
      DateTime eighteenMonths = now.AddMonths(-18);

      var members = this.db.Members.Include("Memberships.Status").Include("Memberships.Unit").Include("ComputedAwards.Course");

      var courses = (from c in this.db.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToList();

      while (!string.IsNullOrWhiteSpace(sheet.CellAt(row, 0).StringValue))
      {
        string dem = sheet.CellAt(row, 1).StringValue.Trim().PadLeft(4, '0');
        string last = sheet.CellAt(row, 2).StringValue.Trim().Split(',')[0];
        string common = sheet.CellAt(row, 4).StringValue.Trim();
        string first = string.IsNullOrWhiteSpace(common) ? sheet.CellAt(row, 3).StringValue.Trim() : common;
        string wacRank = sheet.CellAt(row, 5).StringValue.Trim();
        string active = sheet.CellAt(row, 6).StringValue.Trim();

        if (wacRank == "Canine")
        {

        }
        else
        {
          var nameMatches = members.Where(f => (f.LastName == last || f.LastName.StartsWith(last + ",")) && f.FirstName == first);

          // Not super efficient, but works ok.
          if (nameMatches.Count() > 1)
          {
            nameMatches = members.Where(f => f.LastName == last && f.FirstName == first && f.DEM == dem);
            if (nameMatches.Count() == 0)
            {
              nameMatches = members.Where(f => f.LastName == last && f.FirstName == first);
            }
          }

          string result = dem + " " + last + ", " + first;
          if (nameMatches.Count() == 0)
          {
            model.NoMatch.Add(result + " - Not found");
          }
          else if (nameMatches.Count() > 1)
          {
            model.NoMatch.Add(string.Format("{0} - {1} possible matches", result, nameMatches.Count()));
          }
          else
          {
            var member = nameMatches.First();
            seen.Add(member.Id);

            var trainingStatus = CompositeTrainingStatus.Compute(member, courses, now);

            var units = member.Memberships.Where(f => f.Status.IsActive && f.Activated < now && (f.EndTime == null || f.EndTime.Value > now)).Select(f => f.Unit.DisplayName).Distinct().ToList();
            if (units.Count == 0)
            {
              model.NoMatch.Add(result + " - found in KCSARA database, but has no active unit");
            }

            units.ForEach(f => ReconcileEmergencyWorkersAddMember(model, f,
                new ReconcileEmergencyWorkersRow
                    {
                      Id = member.Id,
                      Name = last + ", " + first,
                      DEM = dem,
                      DemIsNew = (dem != member.DEM),
                      WacIsNew = (wacRank != member.WacLevel.ToString()),
                      WacLevel = wacRank + ((wacRank != member.WacLevel.ToString()) ? ("/" + member.WacLevel.ToString()) : ""),
                      UnitStatus = member.Memberships.Where(g => g.Unit.DisplayName == f).OrderByDescending(g => g.Activated).First().Status.StatusName,
                      IsOldNovice = (wacRank == "Novice" && member.WacLevel == WacLevel.Novice && member.WacLevelDate.AddYears(1) < now),
                      TrainingCurrent = trainingStatus.IsGood,
                      MissionCount = this.db.MissionRosters.Where(g => g.Person.Id == member.Id && g.Mission.StartTime > eighteenMonths).Select(g => g.Mission.Id).Distinct().Count(),
                      TrainingCount = this.db.TrainingRosters.Where(g => g.Person.Id == member.Id && g.Training.StartTime > eighteenMonths).Select(g => g.Training.Id).Distinct().Count()
                    }));
          }
        }
        model.RowCount++;
        row++;
      }

      foreach (var member in members.Where(f => f.Memberships.Any(g => g.Status.IsActive && g.Activated < now && (g.EndTime == null || g.EndTime.Value > now)) && !seen.Contains(f.Id)).OrderBy(f => f.LastName).ThenBy(f => f.FirstName))
      {
        var units = member.Memberships.Where(f => f.Status.WacLevel != WacLevel.None && f.Activated < now && (f.EndTime == null || f.EndTime.Value > now)).Select(f => f.Unit.DisplayName).Distinct().ToList();
        units.ForEach(f => ReconcileEmergencyWorkersAddMember(model, f,
            new ReconcileEmergencyWorkersRow
            {
              Name = member.ReverseName,
              DEM = "",
              DemIsNew = !string.IsNullOrWhiteSpace(member.DEM),
              WacIsNew = (member.WacLevel != WacLevel.None),
              WacLevel = WacLevel.None.ToString(),
              UnitStatus = member.Memberships.Where(g => g.Unit.DisplayName == f).OrderByDescending(g => g.Activated).First().Status.StatusName,
              TrainingCurrent = null,
              MissionCount = this.db.MissionRosters.Where(g => g.Person.Id == member.Id && g.Mission.StartTime > eighteenMonths).Select(g => g.Mission.Id).Distinct().Count(),
              TrainingCount = this.db.TrainingRosters.Where(g => g.Person.Id == member.Id && g.Training.StartTime > eighteenMonths).Select(g => g.Training.Id).Distinct().Count()
            }));
      }

      return View("ReconcileEmergencyWorkerResults", model);
    }

    private void ReconcileEmergencyWorkersAddMember(ReconcileEmergencyWorkersViewModel model, string unit, ReconcileEmergencyWorkersRow row)
    {
      if (!model.MembersByUnit.ContainsKey(unit))
      {
        model.MembersByUnit.Add(unit, new List<ReconcileEmergencyWorkersRow>());
      }

      model.MembersByUnit[unit].Add(row);
    }

    [Authorize]
    public ActionResult PromoteNovices(bool? commit)
    {
      if (!User.IsInRole("cdb.admins")) return GetLoginError();

      DateTime cutoff = DateTime.Today.AddYears(-1);
      var promotions = this.db.Members.Where(f =>
          f.WacLevel == WacLevel.Novice
          && f.WacLevelDate < cutoff
          && f.Memberships.Any(g => g.EndTime == null && g.Status.IsActive)
      ).OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();



      string text = "<p>These members would be promoted to field when this page is called again with &quot;?commit=true&quot; as an argument.</p>";
      if (commit ?? false)
      {
        foreach (var member in promotions)
        {
          member.WacLevel = WacLevel.Field;
          member.WacLevelDate = DateTime.Today;
        }
        this.db.SaveChanges();
        this.db.RecalculateTrainingAwards(promotions);
        this.db.SaveChanges();

        string email = System.Web.Security.Membership.GetUser().Email;
        SendMail(email, "Promotion from Novice to Field",
            string.Format("The following KCSARA members have completed their year as a Novice member and have been automatically moved to Field status.\n\n"
                          + string.Join("\n", promotions.Select(f => f.FullName + (string.IsNullOrWhiteSpace(f.DEM) ? "" : ("  (" + f.DEM + ")"))))
            )
        );
        text = "<p>These members were promoted to Field status. Email has been sent to " + email + "</p>";
      }

      return Content(text + "<table>" + string.Join("", promotions.Select(f => string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", f.DEM, f.LastName, f.FirstName))) + "</table>", "text/html");
    }

    public DataActionResult RecentPhotos(DateTime since)
    {
      if (!User.IsInRole("cdb.users")) return GetLoginError();

      List<RecentDocumentsView> result = new List<RecentDocumentsView>();
      var results = (from u in db.Members where u.LastChanged >= since select new { u.FirstName, u.LastName, u.PhotoFile });

      foreach (var row in results)
      {
        if (row.PhotoFile == null) continue;
        string photoPath = api.MembersController.GetPhotoOrFillInPath(row.PhotoFile);
        FileInfo fInfo = new FileInfo(Server.MapPath(photoPath));
        if (fInfo.LastWriteTime >= since)
        {
          result.Add(new RecentDocumentsView
          {
            Filename = string.Format("{0}, {1}_Photo{2}", row.LastName, row.FirstName, Path.GetExtension(row.PhotoFile)),
            DownloadUrl = Url.Content(photoPath)
          });
        }
      }

      return Data(result.OrderBy(f => f.Filename).ToArray());
    }
  }
}
