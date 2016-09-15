﻿/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity.Infrastructure;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Web.Mvc;
  using System.Xml;
  using Kcsar.Database;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Model;
  using ApiModels = Kcsara.Database.Web.api.Models;

  public partial class TrainingController : SarEventController<Training, TrainingRoster>
  {
    public TrainingController(IKcsarContext db, IAppSettings settings) : base(db, settings) { }

    public override ActionResult Index()
    {
      ViewData["showESAR"] = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["showKCESAR"]);
      return base.Index();
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult ReportStatus()
    {
      IEnumerable<EventReportStatusView> model;
      IQueryable<Training> source = this.db.Trainings;

      var docCount = (from d in this.db.Documents group d by d.ReferenceId into g select new { Id = g.Key, Count = g.Count() }).ToDictionary(f => f.Id, f => f.Count);

      model = (from m in source
               select new EventReportStatusView
               {
                 Id = m.Id,
                 Title = m.Title,
                 Number = m.StateNumber,
                 StartTime = m.StartTime,
                 Persons = m.Roster.Select(f => f.Person.Id).Distinct().Count()
               }).OrderByDescending(f => f.StartTime).ToArray();

      foreach (var r in model)
      {
        int count;
        if (docCount.TryGetValue(r.Id, out count))
        {
          r.DocumentCount = count;
        }
      }

      return View(model);
    }

    protected override void DeleteDependentObjects(Training evt)
    {
      base.DeleteDependentObjects(evt);
      foreach (var roster in this.db.TrainingRosters.Include("TrainingAwards").Where(f => f.Training.Id == evt.Id))
      {
        List<TrainingAward> copy = new List<TrainingAward>(roster.TrainingAwards);
        foreach (var award in copy)
        {
          this.db.TrainingAward.Remove(award);
        }
        this.db.TrainingRosters.Remove(roster);
      }
    }

    #region Training Awards
    [Authorize]
    [HttpGet]
    public ActionResult AwardDetails(Guid? id)
    {
      if (id == null)
      {
        Response.StatusCode = 400;
        return new ContentResult { Content = "No id specified" };
      }

      ApiModels.TrainingRecord award;
      award = (from a in this.db.TrainingAward
               where a.Id == id
               select new
               {
                 Course = new ApiModels.TrainingCourse
                 {
                   Id = a.Course.Id,
                   Title = a.Course.DisplayName
                 },
                 Member = new ApiModels.MemberSummary
                 {
                   Id = a.Member.Id,
                   Name = a.Member.LastName + ", " + a.Member.FirstName,
                   WorkerNumber = a.Member.DEM
                 },
                 Comments = a.metadata,
                 Completed = a.Completed,
                 Expires = a.Expiry,
                 Source = "rule",
                 ReferenceId = a.Id
               }).AsEnumerable().Select(f => new ApiModels.TrainingRecord
               {
                 Course = f.Course,
                 Member = f.Member,
                 Comments = f.Comments,
                 Completed = string.Format(GetDateFormat(), f.Completed),
                 Expires = string.Format(GetDateFormat(), f.Expires),
                 Source = f.Source,
                 ReferenceId = f.ReferenceId
               }).First();
      ViewData["docs"] = (from d in this.db.Documents where d.ReferenceId == award.ReferenceId select new DocumentView { Id = d.Id, Title = d.FileName, Reference = d.ReferenceId, Type = d.Type, Size = d.Size }).ToArray();

      return View(award);
    }
    #endregion

    [Authorize(Roles = "cdb.users")]
    public ActionResult Rules()
    {
      Dictionary<Guid, TrainingCourse> courses = (from c in this.db.TrainingCourses select c).ToDictionary(x => x.Id);

      List<TrainingRule> rules = (from r in this.db.TrainingRules select r).ToList();
      string text = "Rules for Training equivalencies\n=========================\n[]'s after course indicate howmany months" +
      " the equivalency is good for. 'default' means as long as the resultant course is good for.\n" +
      "Mission equivalency indicated by (<required hours>:<of mission type>:<within # months>), '%' means any mission type\n\n\n";
      List<string> lines = new List<string>();
      foreach (TrainingRule rule in rules)
      {
        string line = "";
        string[] fields = rule.RuleText.Split('>');
        if (!fields[0].StartsWith("Mission"))
        {
          Guid?[] sourceCourses = fields[0].Split('+').Select(f => f.ToGuid()).ToArray();

          if (sourceCourses.Any(f => f == null))
          {
            line += "Unknown rule type: " + rule.Id + "\n";
            continue;
          }

          line += string.Join(", ", sourceCourses.Select(f => courses.ContainsKey(f.Value) ? courses[f.Value].DisplayName : f.ToString())) + " => ";
        }
        else
        {
          line += fields[0] + " => ";
        }

        IEnumerable<string> results = fields[1].Split('+');

        string sep = "";
        foreach (string result in results)
        {
          string[] parts = result.Split(':');
          Guid course = new Guid(parts[0]);

          if (!courses.ContainsKey(course))
          {
            line += "Found bad rule: Adds course with ID" + course.ToString() + "\n";
            continue;
          }

          line += sep + courses[course].DisplayName;
          sep = ", ";

          if (parts.Length > 1)
          {
            string validFor = string.Empty;
            if (parts[1] == "default")
            {
              validFor = courses[course].ValidMonths.HasValue ? courses[course].ValidMonths.ToString() : "no-expire";
            }
            else
            {
              validFor = parts[1];
            }
            line += "[" + validFor + "]";
          }
        }
        lines.Add(line);
      }
      return new ContentResult { Content = text + string.Join("\n\n", lines.OrderBy(f => f).ToArray()), ContentType = "text/plain" };
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult CourseList(Guid? unit, int? recent, int? upcoming, bool? filter)
    {
      ViewData["PageTitle"] = "KCSARA :: Course List";
      ViewData["Message"] = "Training Courses";

      ViewData["recent"] = recent = recent ?? 3;
      ViewData["upcoming"] = upcoming = upcoming ?? 3;

      ViewData["filter"] = (filter = filter ?? true);

      ViewData["unit"] = UnitsController.GetUnitSelectList(this.db, unit);
      ViewData["unitFilter"] = unit;

      var courses = (from c in this.db.TrainingCourses select c).ToDictionary(x => x.Id);

      //var model = from s in this.db.GetTrainingExpirationsSummary(recent, upcoming, unit)
      //            select new TrainingCourseSummary { Course = courses[s.CourseId], CurrentCount = s.Good, RecentCount = s.Recent, UpcomingCount = s.Almost, FarExpiredCount = s.Expired };
      var model = from s in this.db.TrainingCourses select new TrainingCourseSummary { Course = s, CurrentCount = 0, RecentCount = 0, UpcomingCount = 0, FarExpiredCount = 0 };


      if ((bool)ViewData["filter"])
      {
        model = model.Where(f => f.Course.WacRequired > 0 || f.Course.ShowOnCard);
      }

      return View(model);
    }

    [Authorize]
    public ActionResult CourseHours(Guid id, DateTime? begin, DateTime? end)
    {
      DateTime e = end ?? DateTime.Now;
      DateTime b = begin ?? e.AddYears(-1);

      TrainingCourseHoursView model = (from c in this.db.TrainingCourses where c.Id == id select new TrainingCourseHoursView { Begin = b, End = e, CourseId = id, CourseName = c.DisplayName }).SingleOrDefault();

      if (model == null)
      {
        return new ContentResult { Content = "Course not found" };
      }

      return View(model);
    }

    [HttpPost]
    public ActionResult GetCourseHours(Guid id, DateTime? begin, DateTime? end)
    {
      if (!User.IsInRole("cdb.users")) return GetLoginError();

      DateTime e = end ?? DateTime.Now;
      DateTime b = begin ?? e.AddYears(-1);

      Dictionary<Guid, MemberRosterRow> memberHours = new Dictionary<Guid, MemberRosterRow>();
      var traineeQuery = (from tr in this.db.TrainingRosters.Include("TrainingAwards.Member").Include("TrainingAwards.Course") where tr.TrainingAwards.Count > 0 && tr.TimeIn >= b && tr.TimeIn < e select tr).ToArray();
      var traineeAwards = traineeQuery.SelectMany(f => f.TrainingAwards).GroupBy(f => new { P = f.Member, Course = f.Course })
          .Select(f => new { Person = f.Key.P, Course = f.Key.Course, Hours = f.Sum(g => g.Roster.Hours), Count = f.Count() });

      var rules = this.db.TrainingRules.ToArray();


      foreach (var award in traineeAwards)
      {
        bool doAward = false;
        if (award.Course.Id == id)
        {
          doAward = true;
        }
        else
        {
          List<Guid> trickleDowns = new List<Guid>(new[] { award.Course.Id });
          int trickleCount = trickleDowns.Count - 1;
          while (trickleCount != trickleDowns.Count)
          {
            trickleCount = trickleDowns.Count;

            foreach (TrainingRule rule in rules)
            {
              if (trickleDowns.Any(f => rule.RuleText.StartsWith(f.ToString() + ">", StringComparison.OrdinalIgnoreCase)))
              {
                foreach (Guid newCourse in rule.RuleText.Split('>')[1].Split('+').Select(f => new Guid(f.Split(':')[0])))
                {
                  if (!trickleDowns.Contains(newCourse)) trickleDowns.Add(newCourse);
                }
              }
            }
          }
          doAward = trickleDowns.Any(f => f == id);
        }

        if (doAward == true)
        {
          if (!memberHours.ContainsKey(award.Person.Id))
          {
            memberHours.Add(award.Person.Id, new MemberRosterRow
            {
              Person = new ApiModels.MemberSummary { Id = award.Person.Id, Name = award.Person.ReverseName, WorkerNumber = award.Person.DEM },
              Count = 0,
              Hours = 0
            });
          }
          memberHours[award.Person.Id].Count += award.Count;
          memberHours[award.Person.Id].Hours += award.Hours ?? 0.0;
        }
      }

      return Data(memberHours.Values.OrderByDescending(f => f.Hours).ThenBy(f => f.Person.Name));
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult CoreCompReport(Guid? id)
    {
      IQueryable<UnitMembership> memberships = this.db.UnitMemberships.Include("Person.ComputedAwards.Course").Include("Status");
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
      var members = memberships.Select(f => f.Person).Distinct().OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      DateTime now = DateTime.Now;
      DateTime lastYear = now.AddYears(-1);
      var missionResponse = db.MissionRosters
        .Where(f => f.TimeIn > lastYear)
        .Select(f => new { MissionId = f.Mission.Id, PersonId = f.Person.Id })
        .Distinct()
        .GroupBy(f => f.PersonId)
        .ToDictionary(f => f.Key, f => f.Count());

      var courses = this.db.GetCoreCompetencyCourses();

      
      var file = ExcelService.Create(ExcelFileType.XLSX);
      var sheet = file.CreateSheet(unitShort);

      var headers = new[] { "DEM #", "Last Name", "First Name", "Field Type", "Missions", "Good Until" }.Union(courses.Select(f => f.DisplayName)).ToArray();
      for (int i = 0; i < headers.Length; i++)
      {
        sheet.CellAt(0, i).SetValue(headers[i]);
        sheet.CellAt(0, i).SetBold(true);
      }

      int row = 1;
      foreach (var member in members)
      {
        sheet.CellAt(row, 0).SetValue(member.DEM);
        sheet.CellAt(row, 1).SetValue(member.LastName);
        sheet.CellAt(row, 2).SetValue(member.FirstName);
        sheet.CellAt(row, 3).SetValue(member.WacLevel.ToString());

        int missionCount;
        sheet.CellAt(row, 4).SetValue(missionResponse.TryGetValue(member.Id, out missionCount) ? missionCount : 0);

        int goodColumn = 5;
        int col = goodColumn + 1;

        DateTime goodUntil = DateTime.MaxValue;
        int coursesCount = 0;

        for (int i = 0; i < courses.Count; i++)
        {
          var match = member.ComputedAwards.SingleOrDefault(f => f.Course.Id == courses[i].Id);
          if (match != null)
          {
            string text = "Complete";
            if (match.Expiry.HasValue)
            {
              if (match.Expiry < goodUntil) goodUntil = match.Expiry.Value;
              sheet.CellAt(row, col + i).SetValue(match.Expiry.Value);

            }
            else
            {
              sheet.CellAt(row, col + i).SetValue(text);
            }
            coursesCount++;
          }
        }
        sheet.CellAt(row, goodColumn).SetValue(courses.Count == coursesCount ? goodUntil : (DateTime?)null);
        row++;
      }
      sheet.AutoFitAll();

      MemoryStream ms = new MemoryStream();
      file.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return this.File(ms, "application/vnd.ms-excel", string.Format("{0}-corecomp-{1:yyyy-MM-dd}.xlsx", unitShort, DateTime.Today));
    }

    [HttpPost]
    public DataActionResult GetMemberExpirations(Guid id)
    {
      if (!Permissions.IsUser && !Permissions.IsSelf(id)) return GetLoginError();

      ApiModels.CompositeExpiration model;
      var courses = (from c in this.db.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToDictionary(f => f.Id, f => f);

      Member m = this.db.Members.Include("ComputedAwards.Course").FirstOrDefault(f => f.Id == id);

      CompositeTrainingStatus stats = CompositeTrainingStatus.Compute(m, courses.Values, DateTime.Now);

      model = new ApiModels.CompositeExpiration
      {
        Goodness = stats.IsGood,
        Expirations = stats.Expirations.Select(f => new ApiModels.TrainingExpiration
        {
          Completed = string.Format(GetDateFormat(), f.Value.Completed),
          Course = new ApiModels.TrainingCourse
          {
            Id = f.Value.CourseId,
            Required = courses[f.Value.CourseId].WacRequired,
            Title = courses[f.Value.CourseId].DisplayName
          },
          Expires = string.Format(GetDateFormat(), f.Value.Expires),
          Status = f.Value.Status.ToString(),
          ExpiryText = f.Value.ToString()
        }).OrderBy(f => f.Course.Title).ToArray()
      };


      return Data(model);
    }

    [Authorize(Roles = "cdb.admins")]
    public ActionResult RecalculateAwards(Guid? id)
    {
      if (id.HasValue)
      {
        this.db.RecalculateTrainingAwards(id.Value);
      }
      else
      {
        this.db.RecalculateTrainingAwards();
      }
      this.db.SaveChanges();
      return new ContentResult { Content = "Done" };
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult Current(Guid id, Guid? unit, bool? expired)
    {
      ViewData["PageTitle"] = "KCSARA :: Training Course";
      ViewData["Course"] = (from c in this.db.TrainingCourses where c.Id == id select c).First();

      ViewData["unit"] = UnitsController.GetUnitSelectList(this.db, unit);
      ViewData["expired"] = (expired = expired ?? false);

      // I'm sure there's a better way to do this, but I'll have to come back to it when I become more of a Linq/Entities/SQL guru.
      // What's here now...
      // SELECT everyone that's taken this course, sorted by name and date. This set will have multiple rows for a person
      // that has taken the course more than once. They should be sorted so that the most recent of these rows comes first.
      // Run through the list, and pull out the earlier records for this person and course.

      Guid lastId = Guid.Empty;
      IQueryable<ComputedTrainingAward> src = this.db.ComputedTrainingAwards.Include("Member").Include("Course").Include("Member.Memberships.Unit").Include("Member.Memberships.Status");

      var model = (from ta in src where ta.Course.Id == id select ta);
      if (!(bool)ViewData["expired"])
      {
        model = model.Where(ta => (ta.Expiry == null || ta.Expiry >= DateTime.Today));
      }
      List<ComputedTrainingAward> awards = model.OrderBy(ta => ta.Member.LastName).ThenBy(f => f.Member.FirstName).ThenBy(f => f.Member.Id).ThenByDescending(f => f.Expiry).ToList();

      if (unit.HasValue)
      {
        awards = awards.Where(f => f.Member.GetActiveUnits().Where(g => g.Unit.Id == unit.Value).Count() > 0).ToList();
      }
      else
      {
        awards = awards.Where(f => f.Member.GetActiveUnits().Count() > 0).ToList();
      }
      return View(awards);
    }

    #region SarEventController base class

    protected override string EventType
    {
      get { return "Training"; }
    }

    protected override bool CanDoAction(SarEventActions action, object context)
    {
      return User.IsInRole("cdb.trainingeditors");
    }

    #endregion

    protected override TrainingRoster AddNewRow(Guid id)
    {
      TrainingRoster row = new TrainingRoster { Id = id };
      this.db.TrainingRosters.Add(row);
      return row;
    }

    protected override void AddEventToContext(Training newEvent)
    {
      this.db.Trainings.Add(newEvent);
    }

    private List<Guid> dirtyAwardMembers = new List<Guid>();

    protected override void OnProcessingRosterInput(TrainingRoster row, FormCollection fields)
    {
      base.OnProcessingRosterInput(row, fields);

      string coursesKey = "courses_" + row.Id.ToString();
      string loweredCourses = (fields[coursesKey] ?? "").ToLower();

      if (!string.IsNullOrEmpty(loweredCourses))
      {
        ModelState.SetModelValue(coursesKey, new ValueProviderResult(fields[coursesKey], fields[coursesKey], CultureInfo.CurrentUICulture));
      }

      TrainingAward[] tmp = row.TrainingAwards.ToArray();

      Dictionary<string, TrainingAward> currentAwards = row.TrainingAwards.ToDictionary(f => f.Course.Id.ToString().ToLower(), f => f);
      bool awardsDirty = false;

      foreach (string award in currentAwards.Keys)
      {
        string lowered = award.ToLower();
        if (loweredCourses.Contains(lowered))
        {
          loweredCourses = loweredCourses.Replace(lowered, "");
        }
        else
        {
          awardsDirty = true;
          this.db.TrainingAward.Remove(currentAwards[award]);
        }
      }

      foreach (string key in loweredCourses.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
      {
        if (!row.TimeOut.HasValue)
        {
          ModelState.AddModelError(coursesKey, "Time out required when awarding courses");
          return;
          //ModelState.AddModelError("TimeOut", "Time out is required if course is awarded.");
          //throw new InvalidOperationException("row's timeout is null");
        }

        if (row.Person == null)
        {
          return;
        }

        if (!ModelState.IsValid)
        {
          return;
        }

        TrainingCourse course = GetCourse(new Guid(key));

        TrainingAward newAward = new TrainingAward()
        {
          Member = row.Person,
          Roster = row,
          Completed = row.TimeOut.Value,
          Course = course,
          Expiry = course.ValidMonths.HasValue ? row.TimeOut.Value.AddMonths(course.ValidMonths.Value) : (DateTime?)null
        };

        awardsDirty = true;
        row.TrainingAwards.Add(newAward);
      }

      if (awardsDirty && row.Person != null && !dirtyAwardMembers.Contains(row.Person.Id))
      {
        dirtyAwardMembers.Add(row.Person.Id);
      }
    }

    protected override void OnRosterPostProcessing()
    {
      base.OnRosterPostProcessing();
      foreach (Guid memberId in dirtyAwardMembers)
      {
        this.db.RecalculateTrainingAwards(memberId);
      }
      dirtyAwardMembers.Clear();
      this.db.SaveChanges();
    }

    protected override void OnDeletingRosterRow(TrainingRoster row)
    {
      base.OnDeletingRosterRow(row);

      Member member = null;

      // Take away any rewards that may have come with this roster row.
      while (row.TrainingAwards.Count > 0)
      {
        this.db.TrainingAward.Remove(row.TrainingAwards.First());
        member = row.Person;
      }

      // Figure out what this means for the rest of the member's training
      if (member != null)
      {
        this.db.RecalculateTrainingAwards(member.Id);
      }
    }

    protected override ActionResult InternalEdit(Training evt)
    {
      var courses = (from c in this.db.TrainingCourses orderby c.DisplayName select c);

      Dictionary<string, string> courseList = courses.ToDictionary(f => f.Id.ToString(), f => f.DisplayName);
      string[] offered = evt.OfferedCourses.Select(f => f.Id.ToString()).ToArray();

      ViewData["OfferedCourses"] = new MultiSelectList(courseList, "Key", "Value", offered);

      var hostUnits = db.Units.ToDictionary(f => f.Id.ToString(), f => f.DisplayName);
      ViewData["HostUnits"] = new MultiSelectList(hostUnits.OrderBy(f => f.Value), "Key", "Value", evt.HostUnits.Select(f => f.Id.ToString()).ToArray());

      return base.InternalEdit(evt);
    }

    protected override void OnProcessingEventModel(Training evt, FormCollection fields)
    {
      base.OnProcessingEventModel(evt, fields);

      evt.HostUnits.Clear();
      if (fields["HostUnits"] != null)
      {
        foreach (SarUnit unit in db.Units)
        {
          if (fields["HostUnits"].ToLower().Contains(unit.Id.ToString()))
          {
            evt.HostUnits.Add(unit);
          }
        }
      }


      evt.OfferedCourses.Clear();
      if (fields["OfferedCourses"] != null)
      {
        foreach (TrainingCourse course in (from c in this.db.TrainingCourses select c))
        {
          if (fields["OfferedCourses"].ToLower().Contains(course.Id.ToString()))
          {
            evt.OfferedCourses.Add(course);
          }
        }
      }

    }

    [Authorize(Roles = "cdb.users")]
    public ContentResult EligibleEmails(Guid? unit, Guid eligibleFor, Guid[] haveFinished)
    {
      List<Guid> allCourses = new List<Guid>(haveFinished);
      allCourses.Add(eligibleFor);

      var mails = ((IObjectContextAdapter)this.db).ObjectContext.ExecuteStoreQuery<string>(string.Format(@"SELECT pc.value
FROM ComputedTrainingAwards cta LEFT JOIN ComputedTrainingAwards cta2
 ON cta.member_id=cta2.member_id AND cta2.course_id='{0}' AND ISNULL(cta2.Expiry, '9999-12-31') >= GETDATE()
 JOIN Members p ON p.id=cta.member_id
 JOIN PersonContacts pc ON pc.person_id=p.id AND pc.type='email'
WHERE cta2.member_id is null and (cta.Expiry IS NULL OR cta.Expiry >= GETDATE()) AND cta.course_id IN ('{1}')
GROUP BY cta.member_id,lastname,firstname,pc.value
HAVING COUNT(cta.member_id) = {2}
ORDER BY lastname,firstname", eligibleFor, string.Join("','", haveFinished.Select(f => f.ToString())), haveFinished.Length));
      return new ContentResult { Content = string.Join("; ", mails), ContentType = "text/plain" };
    }

    #region Course
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateCourse()
    {
      ViewData["PageTitle"] = "New Training Course";

      TrainingCourse c = new TrainingCourse();
      //UnitMembership s = new UnitMembership();
      //s.Person = (from p in this.db.Members where p.Id == personId select p).First();
      //s.Activated = DateTime.Today;

      //Session.Add("NewMembershipGuid", s.Id);
      //ViewData["NewMembershipGuid"] = Session["NewMembershipGuid"];

      return InternalEditCourse(c);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult CreateCourse(FormCollection fields)
    {
      //if (Session["NewMembershipGuid"] != null && Session["NewMembershipGuid"].ToString() != fields["NewMembershipGuid"])
      //{
      //    throw new InvalidOperationException("Invalid operation. Are you trying to re-create a status change?");
      //}
      //Session.Remove("NewMembershipGuid");

      //ViewData["PageTitle"] = "New Unit Membership";

      //UnitMembership um = new UnitMembership();
      //um.Person = (from p in this.db.Members where p.Id == personId select p).First();
      //this.db.AddToUnitMemberships(um);
      //return InternalSaveMembership(um, fields);
      return null;
    }


    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditCourse(Guid id)
    {
      TrainingCourse c = GetCourse(id);
      ViewData["HideFrame"] = true;

      return InternalEditCourse(c);
    }

    private ActionResult InternalEditCourse(TrainingCourse um)
    {
      //SarUnit[] units = (from u in this.db.Units orderby u.DisplayName select u).ToArray();

      //Guid selectedUnit = (um.Unit != null) ? um.Unit.Id : Guid.Empty;

      //// MVC RC BUG - Have to store list in a unique key in order for SelectedItem to work
      //ViewData["Unit"] = new SelectList(units, "Id", "DisplayName", selectedUnit);

      //if (selectedUnit == Guid.Empty && units.Length > 0)
      //{
      //    selectedUnit = units.First().Id;
      //}

      //ViewData["Status"] = new SelectList(
      //        (from s in this.db.UnitStatusTypes.Include("Unit") where s.Unit.Id == selectedUnit orderby s.StatusName select s).ToArray(),
      //        "Id",
      //        "StatusName",
      //        (um.Status != null) ? (Guid?)um.Status.Id : null);

      return View("EditCourse", um);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult EditCourse(Guid id, FormCollection fields)
    {
      ViewData["HideFrame"] = true;
      TrainingCourse c = GetCourse(id);
      return InternalSaveCourse(c, fields);
    }


    private ActionResult InternalSaveCourse(TrainingCourse c, FormCollection fields)
    {
      TryUpdateModel(c, new string[] { "DisplayName", "FullName", "OfferedFrom", "OfferedTo", "ValidMonths", "ShowOnCard", "WacRequired" });

      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        TempData["message"] = "Saved";
        return RedirectToAction("ClosePopup");
      }
      return InternalEditCourse(c);
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteCourse(Guid id)
    {
      return View(GetCourse(id));
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.admins")]
    public ActionResult DeleteCourse(Guid id, FormCollection fields)
    {
      TrainingCourse c = GetCourse(id);
      this.db.TrainingCourses.Remove(c);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }

    private TrainingCourse GetCourse(Guid id)
    {
      return GetCourse(this.db.TrainingCourses, id);
    }

    private TrainingCourse GetCourse(IEnumerable<TrainingCourse> context, Guid id)
    {
      List<TrainingCourse> courses = (from m in context where m.Id == id select m).ToList();
      if (courses.Count != 1)
      {
        throw new ApplicationException(string.Format("{0} training courses found with ID = {1}", courses.Count, id.ToString()));
      }

      TrainingCourse course = courses[0];
      return course;
    }

    #endregion

    [Authorize(Roles = "cdb.users")]
    public FileStreamResult IstTrainingReport(Guid? id, DateTime? date)
    {
      id = id ?? new Guid("c118ce30-cd28-4635-ba3d-adf7c21358e2");

      DateTime today = date ?? DateTime.Today;
      // Take current month and subtract 1 to move to Jan = 0 counting system
      // Take away one more so that reports run during the first month of a new quarter report on last quarter.
      // Then, convert -1 to 12 with +12,%12
      // Divide by 3 months to get the quarter
      int quarter = ((today.Month + 10) % 12) / 3;
      DateTime quarterStart = new DateTime(today.AddMonths(-1).Year, 1, 1).AddMonths(quarter * 3);
      DateTime quarterStop = quarterStart.AddMonths(3);


      ExcelFile file = ExcelService.Create(ExcelFileType.XLS);
      ExcelSheet sheet = file.CreateSheet("IST Members");

      var members = this.db.GetActiveMembers(id, today, "ContactNumbers", "Memberships").OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      var istCourses = new[] { "ICS-300", "ICS-400", "ICS-200", "ICS-800" };
      var courses = this.db.TrainingCourses.Where(f => istCourses.Contains(f.DisplayName) || f.WacRequired > 0).OrderBy(f => f.DisplayName);

      sheet.CellAt(0, 0).SetValue(today.ToString());
      sheet.CellAt(0, 0).SetBold(true);

      var headers = new[] { "Last Name", "First Name", "Ham Call", "Card Type", "Status", "Missing Training", "Mission Ready", string.Format("Q{0} Missions", quarter + 1), string.Format("Q{0} Meetings", quarter + 1) };
      for (int i = 0; i < headers.Length; i++)
      {
        sheet.CellAt(2, i).SetValue(headers[i]);
        sheet.CellAt(2, i).SetBold(true);
      }

      int row = 3;
      foreach (var member in members)
      {
        sheet.CellAt(row, 0).SetValue(member.LastName);
        sheet.CellAt(row, 1).SetValue(member.FirstName);
        sheet.CellAt(row, 2).SetValue(member.ContactNumbers.Where(f => f.Type == "hamcall").Select(f => f.Value).FirstOrDefault());
        sheet.CellAt(row, 3).SetValue(member.WacLevel.ToString());
        sheet.CellAt(row, 4).SetValue(member.Memberships.Where(f => f.Unit.Id == id.Value && f.EndTime == null).Select(f => f.Status.StatusName).FirstOrDefault());


        var expires = CompositeTrainingStatus.Compute(member, courses, today);

        List<string> missingCourses = new List<string>();
        foreach (var course in courses)
        {
          if (!expires.Expirations[course.Id].Completed.HasValue) missingCourses.Add(course.DisplayName);
        }

        sheet.CellAt(row, 5).SetValue(string.Join(", ", missingCourses));



        sheet.CellAt(row, 7).SetValue(member.MissionRosters.Where(f => f.Unit.Id == id && f.TimeIn >= quarterStart && f.TimeIn < quarterStop).Select(f => f.Mission.Id).Distinct().Count());
        var trainingRosters = member.TrainingRosters.Where(f => f.TimeIn >= quarterStart && f.TimeIn < quarterStop).ToList();
        sheet.CellAt(row, 8).SetValue(trainingRosters.Where(f => Regex.IsMatch(f.Training.Title, "IST .*Meeting.*", RegexOptions.IgnoreCase)).Select(f => f.Training.Id).Distinct().Count());

        row++;
      }


      string filename = string.Format("ist-training-{0:yyMMdd}.xls", today);

      MemoryStream ms = new MemoryStream();
      file.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return this.File(ms, "application/vnd.ms-excel", filename);
    }

    [Authorize(Roles = "cdb.users")]
    public FileStreamResult SpartTrainingReport(Guid? id, DateTime? date)
    {
      id = id ?? new Guid("574cb2fe-1acc-4e04-919c-030546b0e7bd");

      DateTime today = date ?? DateTime.Today;
      //// Take current month and subtract 1 to move to Jan = 0 counting system
      //// Take away one more so that reports run during the first month of a new quarter report on last quarter.
      //// Then, convert -1 to 12 with +12,%12
      //// Divide by 3 months to get the quarter
      //int quarter = ((today.Month + 10) % 12) / 3;
      //DateTime quarterStart = new DateTime(today.AddMonths(-1).Year, 1, 1).AddMonths(quarter * 3);
      //DateTime quarterStop = quarterStart.AddMonths(3);


      ExcelFile file = ExcelService.Create(ExcelFileType.XLS);
      ExcelSheet sheet = file.CreateSheet("SPART Members");

      var members = this.db.GetActiveMembers(id, today, "Memberships").OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      var spartCourses = new[] { "OEC", "Avalanche I", "Avalanche II", "MT&R", "MT&R 2" };
      var courses = this.db.TrainingCourses.Where(f => spartCourses.Contains(f.DisplayName) || f.WacRequired > 0);
      var spartCourseGuids = new Guid[spartCourses.Length];
      for (int i = 0; i < spartCourses.Length; i++)
      {
        string courseName = spartCourses[i];
        spartCourseGuids[i] = courses.Where(f => f.DisplayName == courseName).Select(f => f.Id).SingleOrDefault();
      }

      var headers = new[] { "Last Name", "First Name", "OEC", "Avalanche I", "Avalanche II", "MT&R", "MT&R 2", "Missions This Year", "Missions Last Year" };
      for (int i = 0; i < headers.Length; i++)
      {
        sheet.CellAt(0, i).SetValue(headers[i]);
        sheet.CellAt(0, i).SetBold(true);
      }

      int row = 1;
      foreach (var member in members)
      {
        var expires = CompositeTrainingStatus.Compute(member, courses, today);

        int col = 0;
        sheet.CellAt(row, col++).SetValue(member.LastName);
        sheet.CellAt(row, col++).SetValue(member.FirstName);

        foreach (var courseId in spartCourseGuids)
        {
          if (courseId != Guid.Empty)
          {
            var expire = expires.Expirations[(Guid)courseId];
            if (expire.CourseName.StartsWith("Avalanche") && expire.Completed.HasValue)
            {
              expire.Expires = expire.Completed.Value.AddYears(3);
            }
            sheet.CellAt(row, col).SetValue(expire.ToString());
          }
          col++;
        }

        var now = DateTime.Now;
        var yearStart = new DateTime(now.Year, 1, 1);
        var lastYear = new DateTime(now.Year - 1, 1, 1);

        sheet.CellAt(row, col++).SetValue(member.MissionRosters.Where(f => f.Unit.Id == id && f.TimeIn >= yearStart).Select(f => f.Mission.Id).Distinct().Count());
        sheet.CellAt(row, col++).SetValue(member.MissionRosters.Where(f => f.Unit.Id == id && f.TimeIn >= lastYear && f.TimeIn < yearStart).Select(f => f.Mission.Id).Distinct().Count());

        row++;
      }


      string filename = string.Format("spart-training-{0:yyMMdd}.xls", today);

      MemoryStream ms = new MemoryStream();
      file.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return this.File(ms, "application/vnd.ms-excel", filename);
    }


    [Authorize(Roles = "cdb.users")]
    public FileStreamResult EsarTrainingReport()
    {
      DateTime today = DateTime.Today;

      ExcelFile xl;
      using (FileStream fs = new FileStream(Server.MapPath(Url.Content("~/Content/esartraining-template.xls")), FileMode.Open, FileAccess.Read))
      {
        xl = ExcelService.Read(fs, ExcelFileType.XLS);
      }

      ExcelSheet ws = xl.GetSheet(0);

      int row = 0;

      var courseNames = new List<string>(new[] { "Course A", "Course B", "Course C", "NIMS I-100", "NIMS I-700", "Course I", "Course II", "Course III" });
      var courses = (from tc in this.db.TrainingCourses where (tc.Unit.DisplayName == "ESAR" && tc.Categories.Contains("basic")) || tc.DisplayName.StartsWith("NIMS ") orderby tc.DisplayName select tc).ToArray()
          .Where(f => courseNames.Contains(f.DisplayName)).OrderBy(f => courseNames.IndexOf(f.DisplayName)).ToList();


      using (SheetAutoFitWrapper wrap = new SheetAutoFitWrapper(xl, ws))
      {

        foreach (var traineeRow in (from um in this.db.UnitMemberships.Include("Person.ComputedAwards.Course").Include("Person.ContactNumbers")
                                    where um.Unit.DisplayName == "ESAR" && um.Status.StatusName == "trainee" && um.EndTime == null
                                    orderby um.Person.FirstName
                                    orderby um.Person.LastName
                                    select new { p = um.Person, a = um.Person.ComputedAwards, n = um.Person.ContactNumbers }))
        {
          row++;
          Member trainee = traineeRow.p;
          //trainee.ContactNumbers.Add(traineeRow.n);
          //trainee.ComputedAwards.Add(traineeRow.
          //trainee.ContactNumbers.Attach(traineeRow.n);
          //trainee.ComputedAwards.Attach(traineeRow.a);

          int col = 0;
          wrap.SetCellValue(trainee.LastName, row, col++);
          wrap.SetCellValue(trainee.FirstName, row, col++);
          wrap.SetCellValue(trainee.BirthDate.HasValue ? ((trainee.BirthDate.Value.AddYears(21) > DateTime.Today) ? "Y" : "A") : "?", row, col++);
          wrap.SetCellValue(trainee.Gender.ToString().Substring(0, 1), row, col++);
          wrap.SetCellValue(string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "phone" && f.Subtype.ToLowerInvariant() == "home").Select(f => f.Value).ToArray()), row, col++);
          wrap.SetCellValue(string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "phone" && f.Subtype.ToLowerInvariant() == "cell").Select(f => f.Value).ToArray()), row, col++);
          wrap.SetCellValue(string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "email").Select(f => f.Value).ToArray()), row, col++);

          int nextCourseCol = col++;
          bool foundNext = false;

          string courseName = string.Format("{0}", wrap.Sheet.CellAt(0, col).StringValue);
          while (!string.IsNullOrWhiteSpace(courseName))
          {
            var record = trainee.ComputedAwards.FirstOrDefault(f => f.Course.DisplayName == courseName && (f.Expiry == null || f.Expiry > today));

            if (courseName.Equals("Worker App") && trainee.SheriffApp.HasValue)
            {
              wrap.SetCellValue("X", row, col);
            }
            else if (courseName.Equals("BG Check") && trainee.BackgroundDate.HasValue)
            {
              wrap.SetCellValue("X", row, col);
            }
            else if (record != null)
            {
              wrap.SetCellValue(string.Format("{0:yyyy-MM-dd}", record.Completed), row, col);
            }
            else if (foundNext == false)
            {
              wrap.SetCellValue(courseName, row, nextCourseCol);
              foundNext = true;
            }

            col++;
            courseName = string.Format("{0}", wrap.Sheet.CellAt(0, col).StringValue);
          }

          if (trainee.PhotoFile != null) wrap.SetCellValue("havePhoto", row, col++);

          if (Request.QueryString["usernames"] != null && !string.IsNullOrWhiteSpace(trainee.Username)) wrap.SetCellValue(trainee.Username, row, col++);
        }
        wrap.AutoFit();
      }


      string filename = string.Format("esar-training-{0:yyMMdd}.xls", DateTime.Now);

      MemoryStream ms = new MemoryStream();
      xl.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return this.File(ms, "application/vnd.ms-excel", filename);
    }




    protected override void RemoveEvent(Training oldEvent)
    {
      this.db.Trainings.Remove(oldEvent);
    }

    protected override void RemoveRosterRow(TrainingRoster row)
    {
      this.db.TrainingRosters.Remove(row);
    }

    protected override IQueryable<Training> GetEventSource()
    {
      return this.db.Trainings.Include("OfferedCourses").Include("HostUnits").Include("Roster.TrainingAwards");
    }

    [Authorize(Roles = "cdb.trainingeditors")]
    public ActionResult UploadKcsara()
    {
      return View();
    }
  }

  public class TrainingCourseSummary
  {
    public TrainingCourse Course { get; set; }
    public int CurrentCount { get; set; }
    public int UpcomingCount { get; set; }
    public int RecentCount { get; set; }
    public int FarExpiredCount { get; set; }
  }

}
