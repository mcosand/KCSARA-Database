/*
 * Copyright 2008-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity;
  using System.Linq;
  using System.Web.Mvc;
  using Kcsar.Database.Data;
  using Kcsar.Database.Data.Events;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web.Model;

  public partial class MissionsController : SarEventController<MissionRow>
  {
    public MissionsController(IKcsarContext db) : base(db) { }

    [Authorize]
    public override ActionResult Index()
    {
      return List(null);
    }

    #region Subjects
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Mission Id</param>
    /// <returns></returns>
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Subjects(Guid id)
    {
      ViewData["missionId"] = id;
      return View("Subjects", (from b in this.db.SubjectGroups.Include(f => f.SubjectLinks.Select(g => g.Subject)).Include(f => f.Event.SubjectGroups) where b.Event.Id == id select b));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <returns></returns>
    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult SubjectsGroup(Guid id)
    {
      return View("SubjectGroup", (from b in this.db.SubjectGroups where b.Id == id select b).First());
    }

    //[Authorize(Roles="cdb.missioneditors")]
    //[AcceptVerbs(HttpVerbs.Get)]
    //public ActionResult AddSubject(Guid id)
    //{
    //    Subject subject = new Subject();
    //    SubjectGroupLink link = new SubjectGroupLink();
    //    link.Group = (from b in this.db.SubjectGroups where b.Id == id select b).First();
    //    link.Subject = subject;
    //    return View("EditSubject", link);
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <returns></returns>
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult CreateSubject(Guid? id, Guid? missionId)
    {
      return _CreateSubject(id, missionId);
    }

    private ActionResult _CreateSubject(Guid? groupId, Guid? missionId)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["PageTitle"] = "New Subject";

      //Subject subject = new Subject();
      //SubjectGroup group;
      //if (groupId.HasValue)
      //{
      //  ViewData["GroupId"] = groupId.ToString();
      //  group = (from b in this.db.SubjectGroups.Include(f => f.SubjectLinks).Include(f => f.Mission) where b.Id == groupId select b).First();
      //}
      //else
      //{
      //  ViewData["MissionId"] = missionId.ToString();
      //  group = new SubjectGroup { Mission = (from m in this.db.Missions where m.Id == missionId select m).First() };
      //}

      //subject.FirstName = "Unknown";

      //subject.GroupLinks.Add(new SubjectGroupLink { Group = group, Subject = subject });
      ////group.Mission = (from p in this.db.Missions where p.Id == id select p).First();
      ////SubjectGroupLink link = new SubjectGroupLink{

      //Session.Add("NewSubjectGuid", subject.Id);
      //ViewData["NewSubjectGuid"] = Session["NewSubjectGuid"];

      //return InternalEditSubject(subject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Mission ID</param>
    /// <param name="fields"></param>
    /// <returns></returns>
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult CreateSubject(Guid? id, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");

      //if (Session["NewSubjectGuid"] != null && Session["NewSubjectGuid"].ToString() != fields["NewSubjectGuid"])
      //{
      //  throw new InvalidOperationException("Invalid operation. Are you trying to re-create a subject?");
      //}
      //Session.Remove("NewSubjectGuid");

      //ViewData["PageTitle"] = "New Subject";

      //Subject subject = new Subject();
      //this.db.Subjects.Add(subject);

      //Mission mission;
      //SubjectGroup group;

      //if (!string.IsNullOrEmpty(fields["GroupId"]))
      //{
      //  Guid bId = new Guid(fields["GroupId"]);

      //  group = (from b in this.db.SubjectGroups.Include(f => f.Mission).Include(f => f.SubjectLinks) where b.Id == bId select b).First();
      //  mission = group.Mission;
      //}
      //else if (!string.IsNullOrEmpty(fields["MissionId"]))
      //{
      //  Guid mId = new Guid(fields["MissionId"]);
      //  mission = GetEvent(this.db.Missions.Include(f => f.SubjectGroups), mId);
      //  group = new SubjectGroup { Mission = mission, Number = mission.SubjectGroups.Count + 1 };
      //  this.db.SubjectGroups.Add(group);
      //}
      //else
      //{
      //  throw new InvalidOperationException("Must supply either group or mission id");
      //}

      //SubjectGroupLink link = new SubjectGroupLink { Subject = subject, Number = group.SubjectLinks.Count + 1 };
      //group.SubjectLinks.Add(link);
      //this.db.SubjectGroupLinks.Add(link);

      ////MissionLog log = new MissionLog();
      ////log.Mission = (from p in this.db.Missions where p.Id == missionId select p).First();
      ////this.db.AddToMissionLog(log);
      //return InternalSaveSubject(subject, fields);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">subject id</param>
    /// <returns></returns>
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult EditSubject(Guid id)
    {
      // MissionLog log = (from a in this.db.MissionLog.Include("Mission") where a.Id == id select a).First();
      SubjectRow subject = (from s in this.db.Subjects where s.Id == id select s).First();

      return InternalEditSubject(subject);
    }

    private ActionResult InternalEditSubject(SubjectRow subject)
    {
      ViewData["Gender"] = new SelectList(Enum.GetNames(typeof(Gender)), subject.Gender.ToString());
      return View("EditSubject", subject);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult EditSubject(Guid id, FormCollection fields)
    {
      //  MissionLog log = (from a in this.db.MissionLog.Include("Mission") where a.Id == id select a).First();
      //  return InternalSaveLog(log, fields);
      SubjectRow subject = (from s in this.db.Subjects where s.Id == id select s).First();
      return InternalSaveSubject(subject, fields);
    }


    [Authorize(Roles = "cdb.users")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Details(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["missionId"] = id;

      //var details = (from d in this.db.MissionDetails.Include(f => f.Mission) where d.Mission.Id == id select d).FirstOrDefault();

      //if (details == null)
      //{
      //  details = new MissionDetails { Mission = (from m in this.db.Missions where m.Id == id select m).First() };
      //}

      //return View(details);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult EditDetails(Guid id)
    {
      var details = GetDetails(id);
      ViewData["Clouds"] = new MultiSelectList((new[] { "Clear", "Fog", "Overcast", "Partly Cloudy", "Stormy" }).Select(f => new { Text = f }), "Text", "Text", (details.Clouds ?? "").Split(','));

      SetupCheckList(details.Tactics, "EnumTactics", "Tactics", true);
      IEnumerable<string> cluesList = new List<string>(ConfigurationManager.AppSettings["EnumTactics"].Split(','));
      cluesList = cluesList.Union(ConfigurationManager.AppSettings["EnumClues"].Split(','));
      SetupCheckList(details.CluesMethod, cluesList, "CluesMethod", true);

      SetupCheckList(details.TerminatedReason, "EnumTerminate", "TerminateReason", false);

      return View(details);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult EditDetails(Guid id, FormCollection fields)
    {
      EventDetailRow details = GetDetails(id);

      TryUpdateModel(details, new[] { "Clouds", "RainInches", "RainType", "SnowType", "SnowInches", "TempHigh", "TempLow", "Visibility", "WindHigh", "WindLow" });
      TryUpdateModel(details, new[] { "Terrain", "Topography", "GroundCoverDensity", "GroundCoverHeight", "TimberType", "WaterType", "ElevationLow", "ElevationHigh" });

      details.Tactics = ((fields["TacticsList"] ?? "").Replace(',', '|') + '|' + (fields["TacticsOther"] ?? "")).Trim('|');
      details.CluesMethod = ((fields["CluesMethodList"] ?? "").Replace(',', '|') + '|' + (fields["CluesMethodOther"] ?? "")).Trim('|');
      details.TerminatedReason = ((fields["TerminateReasonList"] ?? "").Replace(',', '|') + '|' + (fields["TerminateReasonOther"] ?? "")).Trim('|');

      if (ModelState.IsValid)
      {
        this.db.SaveChanges();
        TempData["message"] = "Saved";
        return RedirectToAction("ClosePopup");
      }

      return EditDetails(details.Event.Id);
    }

    private EventDetailRow GetDetails(Guid missionId)
    {
      throw new NotImplementedException("reimplement");

      //MissionDetails details = (from md in this.db.MissionDetails.Include(f => f.Mission) where md.Mission.Id == missionId select md).FirstOrDefault();
      //if (details == null)
      //{
      //  details = new MissionDetails { Mission = (from m in this.db.Missions where m.Id == missionId select m).First() };
      //  this.db.MissionDetails.Add(details);
      //}
      //return details;
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult EditSubjectGroup(Guid id)
    {
      var group = (from sb in this.db.SubjectGroups.Include(f => f.Event).Include(f => f.SubjectLinks.Select(g => g.Subject)) where sb.Id == id select sb).First();

      SetupCheckList(group.Category, "EnumCategory", "Category", false);
      SetupCheckList(group.Cause, "EnumCause", "Cause", false);
      SetupCheckList(group.Behavior, "EnumBehavior", "Behavior", false);

      IEnumerable<string> foundList = new List<string>(ConfigurationManager.AppSettings["EnumTactics"].Split(','));
      foundList = foundList.Union(ConfigurationManager.AppSettings["EnumFound"].Split(','));
      SetupCheckList(group.FoundTactics, foundList, "Found", true);

      SetupCheckList(group.FoundCondition, "EnumCondition", "Condition", false);
      return View(group);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult EditSubjectGroup(Guid id, FormCollection fields)
    {
      var group = (from sb in this.db.SubjectGroups.Include(f => f.Event).Include(f => f.SubjectLinks.Select(g => g.Subject)) where sb.Id == id select sb).First();

      //try
      //{
        TryUpdateModel(group, new[] { "FoundNorthing", "FoundEasting", "PlsNorthing", "PlsEasting", "PlsCommonName", "WhenAtPls", "WhenCalled", "WhenFound", "WhenLost", "WhenReported", "Comments" });

        group.Category = ((fields["CategoryList"] ?? "").Replace(',', '|') + '|' + (fields["CategoryOther"] ?? "")).Trim('|');
        group.Cause = ((fields["CauseList"] ?? "").Replace(',', '|') + '|' + (fields["CauseOther"] ?? "")).Trim('|');
        group.Behavior = ((fields["BehaviorList"] ?? "").Replace(',', '|') + '|' + (fields["BehaviorOther"] ?? "")).Trim('|');

        group.FoundTactics = ((fields["FoundList"] ?? "").Replace(',', '|') + '|' + (fields["FoundOther"] ?? "")).Trim('|');
        group.FoundCondition = ((fields["ConditionList"] ?? "").Replace(',', '|')).Trim('|');

        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }
      //}
      //catch (RuleViolationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}
      return EditSubjectGroup(group.Id);
    }

    private void SetupCheckList(string modelValue, string appSettingsKey, string viewDataPrefix, bool sort)
    {
      IEnumerable<string> list = new List<string>(ConfigurationManager.AppSettings[appSettingsKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
      SetupCheckList(modelValue, list, viewDataPrefix, sort);
    }

    private void SetupCheckList(string modelValue, IEnumerable<string> list, string viewDataPrefix, bool sort)
    {
      List<string> values = new List<string>();
      List<string> others = new List<string>();

      list = list.Select(f => f.Trim());

      foreach (string value in (modelValue ?? "").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
      {
        string v = value.Trim();

        if (list.Where(f => f.Equals(v, StringComparison.OrdinalIgnoreCase)).Count() > 0)
        {
          values.Add(v);
        }
        else
        {
          others.Add(v);
        }
      }

      if (sort)
      {
        list = list.OrderBy(f => f.Trim());
      }


      ViewData[viewDataPrefix + "List"] = new MultiSelectList(list.Select(f => new { T = f }), "T", "T", values);
      ViewData[viewDataPrefix + "Other"] = string.Join(", ", others.ToArray());
    }

    //[Authorize(Roles = "cdb.missioneditors")]
    //[AcceptVerbs(HttpVerbs.Get)]
    //public ActionResult EditSubjectGroup(SubjectGroup group)
    //{
    //    return View();
    //}

    private ActionResult InternalSaveSubject(SubjectRow subject, FormCollection fields)
    {
      //try
      //{
        TryUpdateModel(subject, new[] { "FirstName", "LastName", "BirthYear", "Gender", "Address", "HomePhone", "WorkPhone", "OtherPhone", "Comments" });


        //    UpdateModel(log, new string[] { "Time", "Data" });

        //    Guid missionId = new Guid(fields["Mission"]);
        //    Mission mission = (from m in this.db.Missions where m.Id == missionId select m).First();
        //    log.Mission = mission;

        //    if (string.IsNullOrEmpty(fields["pid_a"]))
        //    {
        //        ModelState.AddModelError("Person", "Required. Please pick from list.");

        //    }
        //    else
        //    {
        //        Guid personId = new Guid(fields["pid_a"]);
        //        Member person = (from m in this.db.Members where m.Id == personId select m).First();
        //        log.Person = person;
        //    }

        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }
      //}
      //catch (RuleViolationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}

      return InternalEditSubject(subject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">subjectgrouplink id</param>
    /// <returns></returns>
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult DeleteSubject(Guid id)
    {
      SubjectGroupLinkRow link = (from l in this.db.SubjectGroupLinks.Include(f => f.Group.Event).Include(f => f.Subject) where l.Id == id select l).First();
      return View(link);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">subjectgrouplink id</param>
    /// <param name="fields"></param>
    /// <returns></returns>
    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult DeleteSubject(Guid id, FormCollection fields)
    {
      SubjectGroupLinkRow link = (from l in this.db.SubjectGroupLinks.Include(f => f.Group.Event).Include(f => f.Group.SubjectLinks).Include(f => f.Subject.GroupLinks) where l.Id == id select l).First();
      SarEventRow m = link.Group.Event;

      if (link.Subject.GroupLinks.Count == 1)
      {
        this.db.Subjects.Remove(link.Subject);
      }

      SubjectGroupRow group = link.Group;
      if (link.Group.SubjectLinks.Count == 1)
      {
        this.db.SubjectGroups.Remove(link.Group);
        group = null;
      }

      this.db.SubjectGroupLinks.Remove(link);

      if (group != null)
      {
        RenumberSubjects(group);
      }
      RenumberSubjectGroups(m);

      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult MoveSubjectOrder(Guid id, int direction)
    {
      SubjectGroupLinkRow link = (from l in this.db.SubjectGroupLinks.Include(f => f.Group.Event) where l.Id == id select l).First();

      foreach (var x in (from l in this.db.SubjectGroupLinks where l.Group.Id == link.Group.Id && l.Number == (link.Number + direction) select l))
      {
        x.Number -= direction;
      }
      link.Number += direction;

      RenumberSubjects(link.Group);

      this.db.SaveChanges();

      return RedirectToAction("Subjects", new { id = link.Group.Event.Id });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">link id</param>
    /// <param name="newGroup"></param>
    /// <returns></returns>
    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult MoveSubjectToGroup(Guid id, int newGroup)
    {
      SubjectGroupLinkRow link = (from l in this.db.SubjectGroupLinks.Include(f => f.Group.Event) where l.Id == id select l).First();
      SarEventRow m = link.Group.Event;
      SubjectGroupRow oldGroup = link.Group;

      link.Group = (from b in this.db.SubjectGroups where b.Event.Id == link.Group.Event.Id && b.Number == newGroup select b).FirstOrDefault();
      if (link.Group == null)
      {
        link.Group = oldGroup.CreateCopy();
        link.Group.Number = newGroup;

        this.db.SubjectGroups.Add(link.Group);
        RenumberSubjectGroups(m);
      }

      RenumberSubjects(link.Group);
      RenumberSubjects(oldGroup);

      if (oldGroup.SubjectLinks.Count == 0)
      {
        oldGroup.Event.SubjectGroups.Remove(oldGroup);
        this.db.SubjectGroups.Remove(oldGroup);
      }

      this.db.SaveChanges();
      return RedirectToAction("Subjects", new { id = m.Id });
    }

    private void RenumberSubjects(SubjectGroupRow group)
    {
      var navProp = this.db.Entry(group).Reference(f => f.SubjectLinks);
      if (!navProp.IsLoaded) navProp.Load();

      int number = 1;
      foreach (SubjectGroupLinkRow link in group.SubjectLinks.OrderBy(f => f.Number))
      {
        link.Number = number++;
      }
    }

    private void RenumberSubjectGroups(SarEventRow mission)
    {
      //var navProp = this.db.SubjectGroups.l.Entry(mission).Reference(f => f.SubjectGroups);
      //if (!navProp.IsLoaded) navProp.Load();

      int number = 1;
      foreach (SubjectGroupRow group in mission.SubjectGroups.OrderBy(f => f.Number))
      {
        group.Number = number++;
      }
    }
    #endregion

    #region Logs
    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult EditSummary(Guid id)
    {
      ViewData["missionId"] = id;
      EventDetailRow details = GetDetails(id);

      return View(details);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult EditSummary(Guid id, FormCollection fields)
    {
      EventDetailRow details = GetDetails(id);

      //try
      //{
        TryUpdateModel(details, new[] { "Comments" });

        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }
      //}
      //catch (RuleViolationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}

      return EditDetails(details.Event.Id);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult EditExpenses(Guid id)
    {
      ViewData["missionId"] = id;
      EventDetailRow details = GetDetails(id);

      return View(details);
    }

    [Authorize(Roles = "cdb.missioneditors")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult EditExpenses(Guid id, FormCollection fields)
    {
      EventDetailRow details = GetDetails(id);

      //try
      //{
        TryUpdateModel(details, new[] { "EquipmentNotes" });

        if (ModelState.IsValid)
        {
          this.db.SaveChanges();
          TempData["message"] = "Saved";
          return RedirectToAction("ClosePopup");
        }
      //}
      //catch (RuleViolationsException ex)
      //{
      //  this.CollectRuleViolations(ex, fields);
      //}

      return EditDetails(details.Event.Id);
    }


    [Authorize(Roles = "cdb.users")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult ResponderEmails(Guid id, Guid? unitId)
    {
      ViewData["missionId"] = id;
      SarEventRow m = this.GetEvent(this.db.Events.OfType<MissionRow>(), id);

      ViewData["title"] = string.Format("Responder Emails :: {0} {1}", m.StateNumber, m.Title);

      return View(m);
    }


    [Authorize(Roles = "cdb.users")]
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Log(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["missionId"] = id;
      //Mission m = this.GetEvent(GetEventSource().Include(f => f.Log.Select(g => g.Person)).Include(f => f.Details), id);
      //ViewData["title"] = string.Format("Log :: {0} {1}", m.StateNumber, m.Title);
      //return View(m);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.missioneditors")]
    public JsonDataContractResult SubmitLog(LogSubmission log)
    {
      throw new NotImplementedException("reimplement");

      //List<SubmitError> errors = new List<SubmitError>();
      //Guid result = Guid.Empty;

      //MissionLog newLog = null;
      //if (!log.Time.HasValue)
      //{
      //  errors.Add(new SubmitError { Error = "Invalid Date/Time", Property = "Time", Id = new[] { log.Id }}); 
      //}
      //else
      //{
      //  try
      //  {
      //    newLog = new MissionLog
      //    {
      //      Mission = this.db.Missions.Where(f => f.Id == log.MissionId).First(),
      //      Data = log.Message,
      //      Time = log.Time.Value
      //    };
      //    if (log.Person != null)
      //    {
      //      newLog.Person = this.db.Members.Where(f => f.Id == log.Person.Id).First();
      //    }
      //    result = newLog.Id;

      //    this.db.MissionLog.Add(newLog);
      //    this.db.SaveChanges();
      //  }
      //  catch (DbEntityValidationException ex)
      //  {
      //    foreach (var entry in ex.EntityValidationErrors.Where(f => !f.IsValid))
      //    {
      //      foreach (var err in entry.ValidationErrors)
      //      {
      //        errors.Add(new SubmitError { Error = err.ErrorMessage, Property = err.PropertyName, Id = new[] { ((IModelObject)entry.Entry.Entity).Id } });
      //      }
      //    }
      //  }
      //}
      //return new JsonDataContractResult(new SubmitResult<LogSubmission>
      //{
      //  Errors = errors.ToArray(),
      //  Result = (errors.Count > 0) ?
      //      (LogSubmission)null :
      //      new LogSubmission
      //      {
      //        Id = newLog.Id,
      //        Message = newLog.Data,
      //        MissionId = newLog.Mission.Id,
      //        Time = newLog.Time,
      //        Person = (newLog.Person == null) ?
      //            (MemberView)null :
      //            new MemberView
      //            {
      //              Id = newLog.Person.Id,
      //              Name = newLog.Person.ReverseName
      //            }
      //      }
      //});
    }

    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult CreateLog(Guid missionId)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["PageTitle"] = "New Log";

      //MissionLog log = new MissionLog();
      //log.Mission = (from p in this.db.Missions where p.Id == missionId select p).First();

      //Session.Add("NewLogGuid", log.Id);
      //ViewData["NewLogGuid"] = Session["NewLogGuid"];

      //return InternalEditLog(log);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult CreateLog(Guid missionId, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");

      //if (Session["NewLogGuid"] != null && Session["NewLogGuid"].ToString() != fields["NewLogGuid"])
      //{
      //  throw new InvalidOperationException("Invalid operation. Are you trying to re-create a status change?");
      //}
      //Session.Remove("NewLogGuid");

      //ViewData["PageTitle"] = "New Log";

      //MissionLog log = new MissionLog();
      //log.Mission = (from p in this.db.Missions where p.Id == missionId select p).First();
      //this.db.MissionLog.Add(log);
      //return InternalSaveLog(log, fields);
    }

    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult EditLog(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //MissionLog log = (from a in this.db.MissionLog.Include(f => f.Person).Include(f => f.Mission) where a.Id == id select a).First();
      //return InternalEditLog(log);
    }

    private ActionResult InternalEditLog(EventLogRow log)
    {
      return View("EditLog", log);
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult EditLog(Guid id, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");
      //MissionLog log = (from a in this.db.MissionLog.Include(f => f.Mission) where a.Id == id select a).First();
      //return InternalSaveLog(log, fields);
    }


    private ActionResult InternalSaveLog(EventLogRow log, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");

      //TryUpdateModel(log, new string[] { "Time", "Data" });

      //Guid missionId = new Guid(fields["Mission"]);
      //Mission mission = (from m in this.db.Missions where m.Id == missionId select m).First();
      //log.Mission = mission;

      //if (string.IsNullOrEmpty(fields["pid_a"]))
      //{
      //  ModelState.AddModelError("Person", "Required. Please pick from list.");

      //}
      //else
      //{
      //  Guid personId = new Guid(fields["pid_a"]);
      //  Member person = (from m in this.db.Members where m.Id == personId select m).First();
      //  log.Person = person;
      //}

      //if (ModelState.IsValid)
      //{
      //  this.db.SaveChanges();
      //  TempData["message"] = "Saved";
      //  return RedirectToAction("ClosePopup");
      //}

      //return InternalEditLog(log);
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult DeleteLog(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["HideFrame"] = true;
      //return View((from a in this.db.MissionLog.Include(f => f.Person) where a.Id == id select a).First());
    }

    [Authorize]
    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.missioneditors")]
    public ActionResult DeleteLog(Guid id, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");

      //MissionLog log = (from a in this.db.MissionLog where a.Id == id select a).First();
      //this.db.MissionLog.Remove(log);
      //this.db.SaveChanges();

      //return RedirectToAction("ClosePopup");
    }
    #endregion

    #region Documents
    //[HttpGet]
    //public override ActionResult Documents(Guid id)
    //{
    //    List<MissionDocument> model = new List<MissionDocument>();

    //    using (var c = GetContext())
    //    {
    //        Mission m = (from g in c.Missions where g.Id == id select g).First();
    //        ViewData["Title"] = string.Format("Mission Documents{0}", " :: " + m.StateNumber + " " + m.Title);
    //        ViewData["mission"] = m;
    //    }
    //    ViewData["missionId"] = id;


    //    return View(model);
    //}

    //   [HttpPost]
    //   public JsonDataContractResult GetDocuments(Guid id)
    //   {
    //       if (!User.IsInRole("cdb.users")) return GetAjaxLoginError<MapDataView>(null);

    ////       MapDataView model = new MapDataView();
    //       using (var c = GetContext())
    //       {
    //           var query = (from d in c.MissionDocuments where d.Mission.Id == id select d);

    //           model.Items.AddRange(query.AsEnumerable().Select(f => GeographyView.BuildGeographyView(f)));
    //       }
    //       return new JsonDataContractResult(model);
    //   }

    #endregion

    protected override string EventType
    {
      get { return "Missions"; }
    }

    protected override bool CanDoAction(SarEventActions action, object context)
    {
      return this.UserInRole("cdb.missioneditors");
    }

    protected override void OnProcessingEventModel(MissionRow evt, FormCollection fields)
    {
      base.OnProcessingEventModel(evt, fields);
      TryUpdateModel(evt, new string[] { "CountyNumber" }, fields.ToValueProvider());
      if (fields["MissionType"] != evt.MissionType)
      {
        evt.MissionType = fields["MissionType"];
      }
    }

    protected override ActionResult InternalEdit(MissionRow evt)
    {
      ViewData["MissionType"] = new MultiSelectList(ConfigurationManager.AppSettings["EnumMissionType"].Split(','), (evt.MissionType ?? "").Split(','));
      return base.InternalEdit(evt);
    }

    //[Authorize(Roles = "cdb.users")]
    //public override ActionResult List(string id)
    //{
    //  //int year;
    //  //bool useDate = true;
    //  //if ((id ?? "").Equals("all", StringComparison.OrdinalIgnoreCase))
    //  //{
    //  //  useDate = false;
    //  //}
    //  //year = int.TryParse(id, out year) ? year : DateTime.Today.Year;

    //  //Guid? unit = null;
    //  //if (!string.IsNullOrEmpty(Request.Params["unit"]))
    //  //{
    //  //  unit = new Guid(Request.Params["unit"]);
    //  //}

    //  //IEnumerable<EventSummaryView> model;
    //  //DateTime eon = new DateTime(1940, 1, 1);
    //  //var yearsData = this.db.Events.OfType<MissionRow>().Where(f => f.StartTime > eon).Select(f => f.StartTime.Year).Distinct().OrderByDescending(f => f).ToArray();
    //  //ViewData["years"] = yearsData;
    //  //if (yearsData.Length > 0)
    //  //{
    //  //  ViewData["minYear"] = this.db.Events.OfType<MissionRow>().Where(f => f.StartTime > eon).Min(f => f.StartTime).Year;
    //  //  ViewData["maxYear"] = this.db.Events.OfType<MissionRow>().Max(f => f.StartTime).Year;
    //  //}
    //  //else
    //  //{
    //  //  ViewData["minYear"] = DateTime.Now.Year;
    //  //  ViewData["maxYear"] = DateTime.Now.Year;
    //  //}

    //  //IQueryable<MissionRow> source = this.db.Events.OfType<MissionRow>();
    //  //if (unit.HasValue)
    //  //{
    //  //  source = source.Where(f => f.Units.Any(g => g.MemberUnitId == unit.Value));
    //  //}

    //  //if (useDate)
    //  //{
    //  //  DateTime left = new DateTime(year, 1, 1);
    //  //  DateTime right = new DateTime(year + 1, 1, 1);
    //  //  source = source.Where(f => f.StartTime > left && f.StartTime < right);
    //  //  ViewData["year"] = year;
    //  //}

    //  //DateTime recent = DateTime.Now.AddHours(-12);
    //  //model = (from m in source
    //  //         //join mr in this.db.MissionRosters on m.Id equals mr.Mission.Id into Joined
    //  //         //from mr in Joined.DefaultIfEmpty()
    //  //         select m).Distinct().Select(
    //  //          m => new EventSummaryView
    //  //          {
    //  //            Id = m.Id,
    //  //            Title = m.Title,
    //  //            Number = m.StateNumber,
    //  //            StartTime = m.StartTime,
    //  //            Persons = m.Participants.Count(),
    //  //            Hours = m.Roster.Sum(f => f.Hours),
    //  //            Miles = m.Roster.Sum(f => f.Miles),
    //  //            IsActive = (m.StopTime == null || m.StopTime > recent)
    //  //          }).OrderByDescending(f => f.StartTime).ToArray();


    //  //ViewData["unit"] = UnitsController.GetUnitSelectList(this.db, unit);

    //  //ViewData["filtered"] = unit.HasValue;
    //  //return View("List", model);
    //  ViewBag.EventType = this.EventType;
    //  return View("EventList");
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Filter to unit id</param>
    /// <returns></returns>
    [Authorize(Roles = "cdb.users")]
    public ActionResult ReportStatus(string id)
    {
      throw new NotImplementedException("reimplement");

      //Guid? unit = null;
      //if (!string.IsNullOrEmpty(Request.Params["unit"]))
      //{
      //  unit = new Guid(Request.Params["unit"]);
      //}

      //IEnumerable<EventReportStatusView> model;
      //IQueryable<Mission> source = this.db.Missions;
      //if (unit.HasValue)
      //{
      //  source = source.Where(f => f.Roster.Any(g => g.Unit.Id == unit.Value));
      //}

      //var docCount = (from d in this.db.Documents group d by d.ReferenceId into g select new { Id = g.Key, Count = g.Count() }).ToDictionary(f => f.Id, f => f.Count);

      //model = (from m in source
      //         select new EventReportStatusView
      //         {
      //           Id = m.Id,
      //           Title = m.Title,
      //           Number = m.StateNumber,
      //           StartTime = m.StartTime,
      //           Persons = m.Roster.Select(f => f.Person.Id).Distinct().Count(),
      //           GeoCount = m.MissionGeography.Count,
      //           LogCount = m.Log.Count,
      //           SubjectCount = m.SubjectGroups.SelectMany(f => f.SubjectLinks).Count(),
      //           Units = m.Roster.Select(f => f.Unit.DisplayName).Distinct(),
      //           IsTurnaround = (m.MissionType ?? "").Contains("turnaround"),
      //           NotSignedOut = m.Roster.Where(f => f.TimeOut == null).Count()
      //         }).OrderByDescending(f => f.StartTime).ToArray();

      //foreach (var r in model)
      //{
      //  int count;
      //  if (docCount.TryGetValue(r.Id, out count))
      //  {
      //    r.DocumentCount = count;
      //  }
      //}

      //return View(model);
    }

    public DataActionResult GetRosterHours(Guid id, Guid? unitId)
    {
      throw new NotImplementedException("reimplement");

      //if (!User.IsInRole("cdb.users")) return GetLoginError();

      //IQueryable<MissionRoster> source = this.db.MissionRosters;
      //if (unitId.HasValue)
      //{
      //  source = source.Where(f => f.Unit.Id == unitId.Value);
      //}

      //return Data((from r in source
      //             where r.Mission.Id == id
      //             group r by r.Person.Id into g
      //             select
      //                 new MemberRosterRow
      //                 {
      //                   Hours = 1, //g.Sum(f => f.Hours),
      //                   Person = new ApiModels.MemberSummary
      //                   {
      //                     Id = g.Key
      //                   },
      //                   Miles = g.Sum(f => f.Miles) ?? 0
      //                 }).ToArray());
    }

    public ActionResult GetList(int? id, Guid? unit)
    {
      throw new NotImplementedException("reimplement");

      //if (!User.IsInRole("cdb.users")) return GetLoginError();

      //IEnumerable<EventSummaryView> model;
      //IQueryable<Mission> source = this.db.Missions;
      //if (unit.HasValue)
      //{
      //  source = source.Where(f => f.Roster.Any(g => g.Unit.Id == unit.Value));
      //}

      //if (id.HasValue)
      //{
      //  if (id.Value < 1)
      //  {
      //    id = DateTime.Today.Year + id.Value;
      //  }
      //  DateTime start = new DateTime(id.Value, 1, 1);
      //  DateTime stop = new DateTime(id.Value + 1, 1, 1);
      //  source = source.Where(f => f.StartTime >= start && f.StartTime < stop);
      //}

      //model = (from m in source
      //         join mr in this.db.MissionRosters on m.Id equals mr.Mission.Id into Joined
      //         from mr in Joined.DefaultIfEmpty()
      //         select m).Distinct().Select(
      //         m => new EventSummaryView
      //         {
      //           Id = m.Id,
      //           Title = m.Title,
      //           Number = m.StateNumber,
      //           StartTime = m.StartTime,
      //           Persons = m.Roster.Select(f => f.Person.Id).Distinct().Count(),
      //           Hours = m.Roster.Sum(f => SqlFunctions.DateDiff("minute", f.TimeIn, f.TimeOut)) / 60.0,
      //           Miles = m.Roster.Sum(f => f.Miles)
      //         }).OrderByDescending(f => f.StartTime).ToArray();

      //return Data(model);
    }

    //protected override ObjectQuery<Mission> GetSummarySource()
    //{
    //    var src = this.db.Missions;

    //    //if (settings.UnitFilter.Count > 0)
    //    //{
    //    //    src = src.Include("Roster").Include("Roster.Unit");
    //    //    ViewData["filtered"] = true;
    //    //}

    //    return src;

    //}

    //protected override void ApplyListFilter(ref IEnumerable<SarEvent> query)
    //{
    //  throw new NotImplementedException("reimplement");

    //  //base.ApplyListFilter(ref query);

    //  ////if (settings.UnitFilter.Count > 0)
    //  ////{
    //  ////    //    query = query.Where((Func<Mission, bool>)(f => f.Roster.Where(g => settings.UnitFilter.Contains(g.Unit.Id)).Count() > 0));
    //  ////}
    //}

    //protected override IDbSet<Mission> GetEventSource()
    //{
    //    return this.db.Missions;//.Include("Roster.Unit").Include("Roster.Animals").Include("Roster.Person.Animals.Animal");
    //}

    //protected override ObjectQuery<MissionRoster> RostersQuery
    //{
    //    get { return this.db.MissionRosters; }
    //}

    //protected override void AddEventToContext(Mission newEvent)
    //{
    //    this.db.AddToMissions(newEvent);
    //}

    protected override void OnBuildingRosterModel(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //base.OnBuildingRosterModel(id);
      //ViewData["missionId"] = id;
      //ViewData["UnitList"] = (from u in this.db.Units orderby u.County + ":" + u.DisplayName select u).ToArray();
      //ViewData["RoleTypes"] = EventRoster.RoleTypes;

    }

    protected override void OnDeletingRosterRow(EventRosterRow row)
    {
      throw new NotImplementedException("reimplement");

      //while (row.Animals.Count > 0)
      //{
      //  this.db.AnimalMissions.Remove(row.Animals.First());
      //}

      //base.OnDeletingRosterRow(row);
    }

    protected override void OnProcessingRosterInput(EventRosterRow row, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");

      //base.OnProcessingRosterInput(row, fields);

      //// ==== Unit ========================

      //if (string.IsNullOrEmpty(fields["unit_" + row.Id.ToString()]))
      //{
      //  return;
      //}
      //Guid unitId = new Guid(fields["unit_" + row.Id.ToString()]);
      //var unit = (from u in this.db.Units where u.Id == unitId select u).First();

      //if (row.Unit != unit)
      //{
      //  row.Unit = unit;
      //}

      //// ==== Mission Role =================
      //string role = fields["role_" + row.Id.ToString()];
      //if (row.InternalRole != role)
      //{
      //  row.InternalRole = role;
      //}

      //// ==== Overtime hours =================
      //if (!string.IsNullOrEmpty(fields["overtimehours_" + row.Id.ToString()]))
      //{
      //  double hours = double.Parse(fields["overtimehours_" + row.Id.ToString()]); ;
      //  if (hours != row.OvertimeHours)
      //  {
      //    row.OvertimeHours = hours;
      //  }
      //}
      //else if (row.OvertimeHours != null)
      //{
      //  row.OvertimeHours = null;
      //}



      //// ==== Animals ========================
      //string animal = fields["animal_" + row.Id.ToString()];
      //List<AnimalEvent> animalsToRemove = new List<AnimalEvent>();
      //if (string.IsNullOrEmpty(animal))
      //{
      //  foreach (AnimalEvent am in row.Animals)
      //  {
      //    animalsToRemove.Add(am);
      //  }
      //}
      //else
      //{
      //  animal = animal.ToLowerInvariant();

      //  // Figure out which animals should be kept
      //  foreach (AnimalEvent am in row.Animals)
      //  {
      //    string idString = am.Animal.Id.ToString().ToLowerInvariant();
      //    if (!animal.Contains(idString))
      //    {
      //      animalsToRemove.Add(am);
      //    }
      //    else
      //    {
      //      animal = animal.Replace(idString, "");
      //    }
      //  }

      //  // Add the new ones.
      //  foreach (string newAnimalId in animal.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
      //  {
      //    Guid animalId = new Guid(newAnimalId);
      //    Animal a = (from x in this.db.Animals where x.Id == animalId select x).First();
      //    AnimalEvent am = new AnimalEvent { Animal = a, Roster = row };
      //    row.Animals.Add(am);
      //    this.db.AnimalMissions.Add(am);
      //  }
      //}

      //// Get rid of those that should be removed
      //foreach (AnimalEvent am in animalsToRemove)
      //{
      //  this.db.AnimalMissions.Remove(am);
      //}


    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult TopResponders(string unit, string start, string end)
    {
      throw new NotImplementedException("reimplement");

//      DateTime startTime = new DateTime(DateTime.Now.Year, 1, 1);
//      DateTime endTime = new DateTime(DateTime.Now.Year + 1, 1, 1);

//      if (!string.IsNullOrEmpty(start))
//      {
//        if (!DateTime.TryParse(start, out startTime))
//        {
//          ModelState.AddModelError("start", "Invalid date");
//        }
//        ModelState.SetModelValue("start", new ValueProviderResult(start, start, CultureInfo.CurrentUICulture));
//      }

//      if (!string.IsNullOrEmpty(end))
//      {
//        if (!DateTime.TryParse(end, out endTime))
//        {
//          ModelState.AddModelError("end", "Invalid date");
//        }
//        ModelState.SetModelValue("end", new ValueProviderResult(end, end, CultureInfo.CurrentUICulture));
//      }

//      Guid unitId = Guid.Empty;
//      IQueryable<MissionRoster> source = this.db.MissionRosters;
//      if (!string.IsNullOrEmpty(unit))
//      {
//        source = this.db.MissionRosters.Include(f => f.Unit);
//        unitId = new Guid(unit);
//      }

//      SarUnit[] units = (from u in this.db.Units orderby u.DisplayName select u).ToArray();

//      //   Guid selectedUnit = (um.Unit != null) ? um.Unit.Id : Guid.Empty;

//      // MVC RC BUG - Have to store list in a unique key in order for SelectedItem to work
//      ViewData["unit"] = new SelectList(units, "Id", "DisplayName", unitId);


//      var rostersByPerson = (from m in source.Include(f => f.Mission).Include(f => f.Person)
//                             where m.TimeIn >= startTime && m.TimeIn < endTime
//                             orderby m.Person.Id, m.Mission.Id
//                             select m);

//      if (unitId != Guid.Empty)
//      {
//        rostersByPerson = (IOrderedQueryable<MissionRoster>)rostersByPerson.Where(f => f.Unit.Id == unitId);
//      }

//      var totals = new Dictionary<RosterSummaryRow, Member>();

//      Guid lastPerson = Guid.Empty;
//      Guid lastMission = Guid.Empty;
//      RosterSummaryRow currentRow = null;
//      foreach (MissionRoster mr in rostersByPerson)
//      {
//        if (mr.Person.Id != lastPerson)
//        {
//          currentRow = new RosterSummaryRow();
//          totals.Add(currentRow, mr.Person);
//          lastPerson = mr.Person.Id;
//          lastMission = Guid.Empty;
//        }

//        if (mr.Mission.Id != lastMission)
//        {
//          currentRow.Missions++;
//        }

//        currentRow.Hours += mr.Hours.HasValue ? mr.Hours.Value : 0.0;
//        currentRow.Miles += mr.Miles.HasValue ? mr.Miles.Value : 0;

//      }

//      /*
//var subResults = (from mr in this.db.MissionRosters.Include("Person").Include("Mission")
//            where mr.TimeIn > start && mr.TimeIn <= end
//            group mr by new { P = mr.Person, M = mr.Mission } into g
//            select new { Person = g.Key.P, Mission = g.Key.M, Hours = g.Sum(f => f.Hours), Miles = g.Sum(f => f.Miles)});
//var results = (
//from mr in subResults
//orderby mr.Person
//group mr by mr.Person into g
//select new { P = g.Key, Hours = g.Sum(f => f.Hours), Miles = g.Sum(f => f.Miles), Count = g.Count() })
//.ToDictionary(f => f.P, f => new RosterSummaryRow { Hours = f.Hours ?? 0.0, Miles = f.Miles ?? 0, Missions = f.Count });
//*/


//      return View(totals);
    }

    public ActionResult History()
    {
      throw new NotImplementedException("reimplement");

      //DateTime start = new DateTime(DateTime.Now.Year, 1, 1);
      //DateTime end = new DateTime(DateTime.Now.Year + 1, 1, 1);

      //if (!string.IsNullOrEmpty(Request.QueryString["start"]))
      //{
      //  start = DateTime.Parse(Request.QueryString["start"]);
      //}

      //if (!string.IsNullOrEmpty(Request.QueryString["stop"]))
      //{
      //  end = DateTime.Parse(Request.QueryString["stop"]);
      //}

      //var typeHistory = new Dictionary<string, Dictionary<int, int>>();
      //foreach (var row in (from m in this.db.Missions where m.StartTime > KcsarContext.MinEntryDate group m by new { Type = m.MissionType, Year = m.StartTime.Year } into g select new { Type = g.Key.Type, Year = g.Key.Year, Count = g.Count() }))
      //{
      //  foreach (string type in (row.Type ?? "unknown").Split(','))
      //  {
      //    if (!typeHistory.ContainsKey(type))
      //    {
      //      typeHistory.Add(type, new Dictionary<int, int>());
      //    }

      //    if (!typeHistory[type].ContainsKey(row.Year))
      //    {
      //      typeHistory[type].Add(row.Year, row.Count);
      //    }
      //    else
      //    {
      //      typeHistory[type][row.Year] += row.Count;
      //    }
      //  }
      //}
      //ViewData["typeHistory"] = typeHistory;



      //ViewData["volCount"] = (from m in this.db.MissionRosters
      //                        where m.Mission.StartTime > KcsarContext.MinEntryDate
      //                        group m by m.Mission.StartTime.Year into g
      //                        select new { Year = g.Key, Count = g.Select(f => f.Person.Id).Distinct().Count() }
      //                        ).ToDictionary(f => f.Year, f => f.Count);
      //ViewData["volHours"] = (from m in this.db.MissionRosters
      //                        where m.Mission.StartTime > KcsarContext.MinEntryDate
      //                        group m by m.Mission.StartTime.Year into g
      //                        select new { Year = g.Key, Hours = g.Sum(f => SqlFunctions.DateDiff("minute", f.TimeIn, f.TimeOut)) / 60.0 }
      //                        ).ToDictionary(f => f.Year, f => f.Hours);

      //return View();
    }

    public ActionResult Yearly(int? id)
    {
      throw new NotImplementedException("reimplement");

      //ViewData["PageTitle"] = "Mission Statistics for Year " + id.ToString();

      //int year = id ?? DateTime.Now.Year;

      //DateTime start = new DateTime(year, 1, 1);
      //DateTime end = new DateTime(year + 1, 1, 1);

      //if (!string.IsNullOrEmpty(Request.QueryString["start"]))
      //{
      //  start = DateTime.Parse(Request.QueryString["start"]);
      //}

      //if (!string.IsNullOrEmpty(Request.QueryString["stop"]))
      //{
      //  end = DateTime.Parse(Request.QueryString["stop"]);
      //}

      //Dictionary<string, int> typeSummary = new Dictionary<string, int>();

      //foreach (var row in (from m in this.db.Missions
      //                     where m.StartTime > start && m.StartTime < end
      //                     group m by m.MissionType into g
      //                     select new CountSummaryRow { Key = g.Key, Count = g.Count() }))
      //{
      //  string[] types = (row.Key ?? "unknown").Split(',');
      //  foreach (string type in types)
      //  {
      //    if (!typeSummary.ContainsKey(type))
      //    {
      //      typeSummary.Add(type, row.Count);
      //    }
      //    else
      //    {
      //      typeSummary[type] += row.Count;
      //    }
      //  }
      //}

      //ViewData["TypeSummary"] = typeSummary.Select(f => new CountSummaryRow { Key = f.Key, Count = f.Value })
      //                         .OrderBy(x => x.Key).ToArray();

      //var rosterByUnit = (from m in this.db.MissionRosters.Include(f => f.Mission).Include(f => f.Unit).Include(f => f.Person)
      //                    where m.TimeIn >= start && m.TimeIn < end
      //                    orderby m.Unit.DisplayName, m.Unit.Id, m.Person.Id
      //                    select m);


      //SarUnit lastUnit = null;
      //Guid lastPerson = Guid.Empty;
      //Dictionary<SarUnit, RosterSummaryRow> unitTotals = new Dictionary<SarUnit, RosterSummaryRow>();
      //Dictionary<SarUnit, List<Guid>> unitMissions = new Dictionary<SarUnit, List<Guid>>();

      //RosterSummaryRow totalResponse = new RosterSummaryRow();
      //List<Guid> uniquePerson = new List<Guid>();
      //List<Guid> totalMissions = new List<Guid>();

      //foreach (MissionRoster mr in rosterByUnit.ToArray())
      //{
      //  if (mr.Unit != lastUnit)
      //  {
      //    if (!unitTotals.ContainsKey(mr.Unit))
      //    {
      //      unitTotals.Add(mr.Unit, new RosterSummaryRow());
      //    }
      //    lastPerson = Guid.Empty;
      //    lastUnit = mr.Unit;
      //  }

      //  if (!totalMissions.Contains(mr.Mission.Id))
      //  {
      //    totalMissions.Add(mr.Mission.Id);
      //  }

      //  if (!unitMissions.ContainsKey(mr.Unit))
      //  {
      //    unitMissions.Add(mr.Unit, new List<Guid>());
      //  }
      //  if (!unitMissions[mr.Unit].Contains(mr.Mission.Id))
      //  {
      //    unitMissions[mr.Unit].Add(mr.Mission.Id);
      //  }

      //  if (mr.Person.Id != lastPerson)
      //  {
      //    if (!uniquePerson.Contains(mr.Person.Id))
      //    {
      //      uniquePerson.Add(mr.Person.Id);
      //    }
      //    unitTotals[mr.Unit].Persons++;
      //    lastPerson = mr.Person.Id;
      //  }

      //  if (mr.TimeIn.HasValue && mr.TimeOut.HasValue)
      //  {
      //    double hours = (mr.TimeOut.Value - mr.TimeIn.Value).TotalHours;
      //    unitTotals[mr.Unit].Hours += hours;
      //    totalResponse.Hours += hours;
      //  }

      //  if (mr.Miles.HasValue)
      //  {
      //    int miles = mr.Miles.Value;
      //    unitTotals[mr.Unit].Miles += miles;
      //    totalResponse.Miles += miles;
      //  }
      //}

      //foreach (SarUnit unit in unitMissions.Keys)
      //{
      //  unitTotals[unit].Missions = unitMissions[unit].Count;
      //  unitTotals[unit].Title = unit.DisplayName;
      //  unitTotals[unit].Id = unit.Id;
      //}

      //totalResponse.Missions = totalMissions.Count;
      //totalResponse.Persons = uniquePerson.Count;

      //ViewData["UnitsSummary"] = unitTotals.Values.OrderBy(x => x.Title).ToArray();
      //ViewData["TotalSummary"] = totalResponse;

      //return View();
    }

    [Authorize]
    public ViewResult RespondersWithExpiredTraining(DateTime? start, DateTime? stop)
    {
      stop = stop ?? DateTime.Now;
      start = start ?? stop.Value.AddMonths(-6);

      ViewData["start"] = start;
      ViewData["stop"] = stop;
      return View((MissionRosterWithExpiredTrainingView[])GetMissionRostersWithExpiredTraining(start, stop).Data);
    }

    [Authorize]
    public DataActionResult GetMissionRostersWithExpiredTraining(DateTime? start, DateTime? stop)
    {
      throw new NotImplementedException("reimplement");

      //stop = stop ?? DateTime.Now;
      //start = start ?? stop.Value.AddMonths(-6);

      //List<MissionRosterWithExpiredTrainingView> rows = new List<MissionRosterWithExpiredTrainingView>();

      //using (KcsarContext sandbox = new KcsarContext())
      //{
      //  var rosters = (from mr in this.db.MissionRosters.Include(f => f.Mission).Include(f => f.Person) where mr.Mission.StartTime < stop.Value && mr.Mission.StartTime >= start.Value select mr);

      //  var courses = (from c in sandbox.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToList();


      //  foreach (var roster in rosters.OrderBy(f => f.Person.LastName + f.Person.FirstName))
      //  {
      //    DateTime time = roster.TimeIn ?? roster.Mission.StartTime;
      //    Member m = sandbox.Members.Include("ComputedAwards").Single(f => f.Id == roster.Person.Id);

      //    string debug = m.ReverseName + "\n";


      //    var computed = sandbox.RecalculateTrainingAwards(new[] { m }, time)[0];
      //    debug += time.ToString("yyMMdd: ");

      //    foreach (var course in courses)
      //    {
      //      var tmp = computed.Where(f => f.Course.Id == course.Id).Select(f => f.Expiry).FirstOrDefault();
      //      debug += (tmp == null) ? "       " : tmp.Value.ToString("yyMMdd ");
      //    }

      //    var requiredCourses = CompositeTrainingStatus.Compute(
      //        m,
      //        computed,
      //        courses,
      //        time
      //        );

      //    if (!requiredCourses.IsGood)
      //    {
      //      System.Diagnostics.Debug.WriteLine(debug);
      //      System.Diagnostics.Debug.Write("        ");

      //      rows.Add(new MissionRosterWithExpiredTrainingView
      //      {
      //        Member = new ApiModels.MemberSummary(roster.Person),
      //        Mission = new EventSummaryView(roster.Mission),
      //        ExpiredTrainings = requiredCourses.Expirations.Where(f => ((f.Value.Status & ExpirationFlags.Okay) != ExpirationFlags.Okay)).Select(f => f.Value.CourseName).ToArray()
      //      });

      //      foreach (var course in courses)
      //      {
      //        var tmp = requiredCourses.Expirations[course.Id].Expires;
      //        System.Diagnostics.Debug.Write((tmp == null) ? "       " : tmp.Value.ToString("yyMMdd "));
      //      }
      //      System.Diagnostics.Debug.WriteLine("");
      //    }
      //  }
      //}
      //return Data(rows.ToArray());
    }
  }
}
