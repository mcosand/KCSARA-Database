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

    [AuthorizeWithLog(Roles = "cdb.users")]
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
    [AuthorizeWithLog]
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

    [AuthorizeWithLog(Roles = "cdb.users")]
    public ActionResult Rules()
    {
      Dictionary<Guid, TrainingCourse> courses = (from c in this.db.TrainingCourses select c).ToDictionary(x => x.Id);
      Dictionary<Guid, SarUnit> units = (from u in db.Units select u).ToDictionary(x => x.Id);

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
          Tuple<Guid?,bool>[] sourceCourses = fields[0].Split('+').Select(f => f.StartsWith("!") ? new Tuple<Guid?,bool>(f.Substring(1).ToGuid(), true) : new Tuple<Guid?,bool>(f.ToGuid(), false)).ToArray();

          if (sourceCourses.Any(f => f == null))
          {
            line += "Unknown rule type: " + rule.Id + "\n";
            continue;
          }

          line += string.Join(", ", sourceCourses.Select(f => 
            (courses.ContainsKey(f.Item1.Value) ? courses[f.Item1.Value].DisplayName : f.ToString())
            + (f.Item2 ? "[Strict]" : "")
            )) + " => ";
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
        if (rule.OfferedFrom.HasValue)
        {
          line += $"[from {rule.OfferedFrom.Value.ToString("d")}]";
        }
        if (rule.OfferedUntil.HasValue)
        {
          line += $"[until {rule.OfferedUntil.Value.ToString("d")}]";
        }
        if (rule.UnitId.HasValue)
        {
          line += $"[{units[rule.UnitId.Value].DisplayName} only]";
        }
        lines.Add(line);
      }
      return new ContentResult { Content = text + string.Join("\n\n", lines.OrderBy(f => f).ToArray()), ContentType = "text/plain" };
    }

    public ActionResult CourseList(Guid? unit, int? recent, int? upcoming, bool? filter)
    {
      return RedirectPermanent(Url.Content("~/training/courses"));
    }

    [AuthorizeWithLog]
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

    [AuthorizeWithLog(Roles = "cdb.users")]
    public ActionResult CoreCompReport(Guid? id)
    {
      IQueryable<UnitMembership> memberships = this.db.UnitMemberships.Include("Person.ComputedAwards.Course").Include("Status");
      string unitShort = ConfigurationManager.AppSettings["site:groupAcronym"] ?? "KCSARA";
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

        DateTimeOffset goodUntil = DateTimeOffset.MaxValue;
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
              sheet.CellAt(row, col + i).SetValue(match.Expiry.Value.LocalDateTime);

            }
            else
            {
              sheet.CellAt(row, col + i).SetValue(text);
            }
            coursesCount++;
          }
        }
        sheet.CellAt(row, goodColumn).SetValue(courses.Count == coursesCount ? goodUntil.LocalDateTime : (DateTime?)null);
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

    [AuthorizeWithLog(Roles = "cdb.admins")]
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

    public ActionResult Current(Guid id, Guid? unit, bool? expired)
    {
      return RedirectPermanent(Url.Content("~/training/courses/" + id.ToString() + "/roster"));
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

    [AuthorizeWithLog(Roles = "cdb.users")]
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

    [AuthorizeWithLog(Roles = "cdb.trainingeditors")]
    public ActionResult UploadKcsara()
    {
      return RedirectPermanent(Url.Content("~/training/uploadrecords"));
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
