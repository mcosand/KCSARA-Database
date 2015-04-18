/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Web.Mvc;
  using Kcsar.Database.Data;
  using Kcsar.Database.Data.Events;
  using Kcsara.Database.Web.Model;

  public abstract class SarEventController<E> : BaseController
    where E : SarEventRow, new()
  {
    public SarEventController(IKcsarContext db) : base(db) { }

    [Authorize(Roles = "cdb.users")]
    public virtual ActionResult Index()
    {
      return View("Index");
    }

    [Authorize(Roles = "cdb.users")]
    public ActionResult Detail(Guid id)
    {
      ViewBag.EventId = id;
      ViewBag.ControllerName = this.EventType;
      return View("EventDetail");
    }

    [Authorize(Roles = "cdb.users")]
    public virtual ActionResult List(string id)
    {
      ViewBag.EventType = this.EventType;
      return View("EventList");
    }

    #region Event CRUD
    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Create()
    {
      if (!this.CanDoAction(SarEventActions.CreateEvent, null))
      {
        return this.CreateLoginRedirect();
      }

      ViewData["PageTitle"] = "New " + this.EventType;
      this.SetSessionValue("NewEventGuid", Guid.NewGuid());
      ViewData["NewEventGuid"] = this.GetSessionValue("NewEventGuid");

      E sarEvent = new E();

      return InternalEdit(sarEvent);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Create(FormCollection fields)
    {
      if (!this.CanDoAction(SarEventActions.CreateEvent, null))
      {
        return this.CreateLoginRedirect();
      }

      if (this.GetSessionValue("NewEventGuid") != null && this.GetSessionValue("NewEventGuid").ToString() != fields["NewEventGuid"])
      {
        throw new InvalidOperationException("Invalid operation. Are you trying to re-create a " + this.EventType + "?");
      }
      this.SetSessionValue("NewEventGuid", null);

      ViewData["PageTitle"] = "New " + this.EventType;

      E evt = new E();
      this.db.Events.Add(evt);

      return InternalSave(evt, fields, RedirectToAction("Roster", new { id = evt.Id, edit = true }));
    }

    [AcceptVerbs("GET")]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Edit(Guid id)
    {
      if (!this.CanDoAction(SarEventActions.UpdateEvent, null))
      {
        return this.CreateLoginRedirect();
      }

      ViewData["HideFrame"] = true;

      return InternalEdit(GetEvent(id));
    }

    [AcceptVerbs("POST")]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Edit(Guid id, FormCollection fields)
    {
      if (!this.CanDoAction(SarEventActions.UpdateEvent, null))
      {
        return this.CreateLoginRedirect();
      }

      ViewData["HideFrame"] = true;

      return InternalSave(this.GetEvent(id), fields, RedirectToAction("ClosePopup"));
    }

    protected virtual ActionResult InternalEdit(E evt)
    {
      ViewData["County"] = new SelectList(Utils.CountyNames.Select(f => new { Text = f, Value = f.ToLower() }).OrderBy(x => x.Text), "Value", "Text", (evt.County ?? "").ToLower());

      return View("Edit", evt);
    }

    protected virtual void OnProcessingEventModel(E evt, FormCollection fields)
    {
    }

    private ActionResult InternalSave(E evt, FormCollection fields, ActionResult successResult)
    {
      try
      {
        TryUpdateModel(evt, new string[] { "Title", "County", "Location", "StateNumber" }, fields.ToValueProvider());

        DateTime newTime;
        if (string.IsNullOrEmpty(fields["StartTime"]))
        {
          ModelState.SetModelValue("StartTime", new ValueProviderResult(fields["StartTime"], fields["StartTime"], CultureInfo.CurrentUICulture));
          ModelState.AddModelError("StartTime", "Required");
        }
        else if (TryParseSarDate(fields["StartTime"], out newTime))
        {
          if (newTime != evt.StartTime)
          {
            evt.StartTime = newTime;
          }
        }
        else
        {
          ModelState.SetModelValue("StartTime", new ValueProviderResult(fields["StartTime"], fields["StartTime"], CultureInfo.CurrentUICulture));
          ModelState.AddModelError("StartTime", "Can't understand date. Use a different format.");
        }

        if (string.IsNullOrEmpty(fields["StopTime"]))
        {
          if (evt.StopTime != null)
          {
            evt.StopTime = null;
          }
        }
        else if (TryParseSarDate(fields["StopTime"], out newTime))
        {
          if (newTime != evt.StopTime)
          {
            evt.StopTime = newTime;
          }
        }
        else
        {
          ModelState.SetModelValue("StopTime", new ValueProviderResult(fields["StopTime"], fields["StopTime"], CultureInfo.CurrentUICulture));
          ModelState.AddModelError("StopTime", "Can't understand date. Use a different format.");
        }

        OnProcessingEventModel(evt, fields);
        if (!ModelState.IsValid)
        {
          return InternalEdit(evt);
        }

        this.db.SaveChanges();

        return successResult;
      }
      catch (InvalidOperationException)
      {
        return InternalEdit(evt);
      }
      ////catch (RuleViolationsException ex)
      ////{
      ////  this.CollectRuleViolations(ex, fields);
      //  return InternalEdit(evt);
      ////}
    }

    private bool TryParseSarDate(string s, out DateTime date)
    {
      // Allow people to enter times on a roster as HHmm, and they'll want to do it everywhere.
      // Lots of people are now trying to use "mm/dd/yy HHmm" or similar format, when they should
      // be using "mm/dd/yy HH:mm". We'll try adding it in for them if needed.
      date = DateTime.MinValue;

      if (string.IsNullOrEmpty(s))
      {
        return false;
      }

      // Happy path - user provides a common parsable date.
      if (DateTime.TryParse(s, out date))
      {
        return true;
      }

      // Special case - user needed to use "HH:mm" instead of "HHmm"
      Match m = Regex.Match(s, @"^(.* \d?\d)(\d\d)$");
      if (m.Success)
      {
        if (DateTime.TryParse(m.Groups[1].Value + ':' + m.Groups[2], out date))
        {
          return true;
        }
      }

      return false;
    }

    [AcceptVerbs(HttpVerbs.Get)]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Delete(Guid id)
    {
      E evt = this.GetEvent(id);

      if (!this.CanDoAction(SarEventActions.DeleteEvent, evt))
      {
        return this.CreateLoginRedirect();
      }

      return View(evt);
    }

    [AcceptVerbs(HttpVerbs.Post)]
    [Authorize(Roles = "cdb.users")]
    public ActionResult Delete(Guid id, FormCollection fields)
    {
      E evt = this.GetEvent(id);

      if (!this.CanDoAction(SarEventActions.DeleteEvent, evt))
      {
        return this.CreateLoginRedirect();
      }

      DeleteDependentObjects(evt);

      this.db.Events.Remove(evt);
      this.db.SaveChanges();

      return RedirectToAction("ClosePopup");
    }
    #endregion

    protected virtual void DeleteDependentObjects(E evt)
    {

    }

    protected abstract string EventType { get; }
    protected abstract bool CanDoAction(SarEventActions action, object context);

    ///// <summary>
    ///// Retrieves all of the details for a user.
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns></returns>
    //[Authorize(Roles = "cdb.users")]
    //[AcceptVerbs(HttpVerbs.Get)]
    //public ActionResult Roster(Guid id)
    //{
    //  var model = BuildRosterModel(id);
      
    //  ViewData["TargetController"] = (string)this.RouteData.Values["controller"];
    //  ViewData["CanEdit"] = CanDoAction(SarEventActions.UpdateEvent, model.SarEvent);

    //  ViewData["CanEditRoster"] = CanDoAction(SarEventActions.AddRosterRow, model.SarEvent)
    //                           || CanDoAction(SarEventActions.DeleteRosterRow, model.SarEvent)
    //                           || CanDoAction(SarEventActions.UpdateRosterRow, model.SarEvent);

    //  ViewData["Title"] = "Roster::" + model.SarEvent.Title;
    //  ViewData["trainingId"] = id;
    //  return View(model);
    //}


    ///// <summary>
    ///// Retrieves all of the details for a user.
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns></returns>
    //[Authorize(Roles = "cdb.users")]
    //[AcceptVerbs(HttpVerbs.Get)]
    //public ActionResult EditRoster(Guid id)
    //{
    //  if (!this.CanDoAction(SarEventActions.UpdateRosterRow, null))
    //  {
    //    return this.CreateLoginRedirect();
    //  }

    //  ExpandedRowsContext rows = BuildRosterModel(id);

    //  for (int i = 0; i < 10; i++)
    //  {
    //    rows.Rows.Add(new EventRosterRow());
    //  }

    //  return View(rows);
    //}

    //protected ExpandedRowsContext BuildRosterModel(Guid eventId)
    //{
    //  E sarEvent = GetEvent(this.db.Events/*.Include("Roster").Include("Roster.Person")*/.OfType<E>(), eventId);
    //  ExpandedRowsContext rows = GetRosterRows(sarEvent);

    //  OnBuildingRosterModel(eventId);
    //  return rows;
    //}

    //protected virtual void OnBuildingRosterModel(Guid id) { }
    //protected virtual void OnDeletingRosterRow(EventRosterRow row) { }
    //protected virtual void OnProcessingRosterInput(EventRosterRow row, FormCollection fields) { }
    //protected virtual void OnRosterPostProcessing() { }

    //     protected abstract ObjectQuery<R> RostersQuery { get; }

    [Authorize(Roles = "cdb.users")]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult EditRoster(Guid id, FormCollection fields)
    {
      throw new NotImplementedException("reimplement");
      //if (!this.CanDoAction(SarEventActions.UpdateRosterRow, null))
      //{
      //  return this.CreateLoginRedirect();
      //}

      //ExpandedRowsContext rows = BuildRosterModel(id);

      //rows.RosterStart = DateTime.Parse(fields["StartDay"]);
      //rows.NumDays = int.Parse(fields["DayCount"]);

      //// Because we're using a SubmitImage, the field is sent twice, with .x and .y suffixes.
      //if (!string.IsNullOrEmpty(fields["AddDayRight.x"]))
      //{
      //  rows.NumDays++;
      //}
      //else if (!string.IsNullOrEmpty(fields["AddDayLeft.x"]))
      //{
      //  rows.NumDays++;
      //  rows.RosterStart = rows.RosterStart.AddDays(-1);
      //}
      //else
      //{
      //  // Get the model of the roster - already in 'rows'
      //  // Foreach roster row in input data
      //  foreach (string key in fields.AllKeys)
      //  {
      //    Match m = Regex.Match(key, @"^id_([a-fA-F\-\d]+)$", RegexOptions.IgnoreCase);

      //    if (!m.Success)
      //    {
      //      continue;
      //    }

      //    Guid rowId = new Guid(fields[key]);
      //    bool isNewRow = false;
      //    string nameKey = "name_" + rowId.ToString();

      //    // Skip new rows without information (name is blank)
      //    // New rows without a person name won't be parsed.
      //    if (string.IsNullOrEmpty(fields[nameKey]))
      //    {
      //      continue;
      //    }


      //    // If the row isn't in the model, then it's one we're adding.
      //    EventRoster row = rows.Rows.Where(x => x.Id == rowId).FirstOrDefault();
      //    if (row == null)
      //    {
      //      row = this.AddNewRow(rowId);
      //      throw new NotImplementedException("reimplement");
      //      // row.SetEvent(rows.SarEvent);
      //      rows.Rows.Add(row);
      //      isNewRow = true;
      //    }

      //    // Now have a model for the row itself

      //    // If the row is to be deleted (box is checked), then delete it.
      //    bool delete = false;
      //    if (bool.TryParse(fields["del_" + rowId.ToString()].Split(',')[0], out delete))
      //    {
      //      if (delete)
      //      {
      //        ModelState.SetModelValue("del_" + rowId.ToString(), new ValueProviderResult(delete.ToString(), delete.ToString(), CultureInfo.CurrentUICulture));
      //        OnDeletingRosterRow(row);
      //        RemoveRosterRow(row);
      //        rows.Rows.Remove(row);
      //        continue;
      //      }
      //    }
      //    // Update the row model with values from the POST
      //    DateTime? newTime;
      //    if (ParseDateValue(rows.RosterStart, rows.NumDays, rowId, "in", fields, out newTime) && (newTime != row.TimeIn))
      //    {
      //      row.TimeIn = newTime.Value;
      //    }

      //    if (ParseDateValue(rows.RosterStart, rows.NumDays, rowId, "out", fields, out newTime) && (newTime != row.TimeOut))
      //    {
      //      row.TimeOut = newTime;
      //    }

      //    int? newMiles;
      //    string milesKey = "miles_" + rowId.ToString();
      //    if (ParseMiles(milesKey, fields[milesKey], out newMiles) && newMiles != row.Miles)
      //    {
      //      row.Miles = newMiles;
      //    }


      //    Guid personId = new Guid(fields["pid_" + rowId.ToString()]);

      //    if (!isNewRow && personId == row.Person.Id && fields[nameKey] == row.Person.ReverseName)
      //    {
      //      // No change
      //    }
      //    else if (isNewRow || personId != row.Person.Id)
      //    {
      //      Member person = (from mbr in this.db.Members where mbr.Id == personId select mbr).FirstOrDefault();
      //      if (person != null && person.ReverseName == fields[nameKey])
      //      {
      //        row.Person = person;
      //      }
      //      else
      //      {
      //        // Unrecognized name
      //        ModelState.AddModelError(nameKey, "Non-members not supported. Please pick from the list");
      //        ModelState.SetModelValue(nameKey, new ValueProviderResult(fields[nameKey], fields[nameKey], CultureInfo.CurrentUICulture));
      //      }
      //    }
      //    else
      //    {
      //      // ids are same, but name doesn't match -
      //      ModelState.AddModelError(nameKey, "Non-members not supported. Please pick from the list");
      //      ModelState.SetModelValue(nameKey, new ValueProviderResult(fields[nameKey], fields[nameKey], CultureInfo.CurrentUICulture));
      //    }

      //    // Ask derived classes to further update the model
      //    OnProcessingRosterInput(row, fields);

      //  }

      //  ViewData["error"] = "Please correct errors";
      //  // Try to commit changes to the store
      //  if (ModelState.IsValid)
      //  {
      //    try
      //    {
      //      this.db.SaveChanges();

      //      OnRosterPostProcessing();

      //      // Return to editing more rows.
      //      ViewData["error"] = null;
      //      ViewData["success"] = "Roster saved.";

      //      if (!string.IsNullOrEmpty(fields["button"]) && ((string)fields["button"]).Equals(Strings.FinishRoster, StringComparison.OrdinalIgnoreCase))
      //      {
      //        return RedirectToAction("Roster", new { id = rows.EventId });
      //      }
      //    }
      //    catch (DbEntityValidationException ex)
      //    {
      //      foreach (var entry in ex.EntityValidationErrors.Where(f => !f.IsValid))
      //      {
      //        foreach (var err in entry.ValidationErrors)
      //        {
      //          if (err.PropertyName == "TimeIn" || err.PropertyName == "TimeOut")
      //          {
      //            bool flagged = false;
      //            for (int i = 0; i < rows.NumDays; i++)
      //            {
      //              string key = err.PropertyName.Substring(4).ToLower() + rows.RosterStart.AddDays(i).ToString("yyMMdd") + "_" + ((IModelObject)entry.Entry.Entity).Id.ToString();

      //              // Only flag the boxes with text in them.
      //              // If none have text, mark the last one in the row.
      //              if (!string.IsNullOrEmpty(fields[key]) || (!flagged && i == (rows.NumDays - 1)))
      //              {
      //                ModelState.SetModelValue(key, new ValueProviderResult(fields[key], fields[key], CultureInfo.CurrentUICulture));
      //                ModelState.AddModelError(key, err.ErrorMessage);
      //                flagged = true;
      //              }
      //            }
      //          }
      //          else
      //          {
      //            string key = err.PropertyName + "_" + ((IModelObject)entry.Entry.Entity).Id.ToString();
      //            ModelState.SetModelValue(key, new ValueProviderResult(fields[key], fields[key], CultureInfo.CurrentUICulture));
      //            ModelState.AddModelError(key, err.ErrorMessage);
      //          }
      //        }
      //      }
      //    }

      //    // Return to editing view
      //  }
      //}

      //for (int i = 0; i < 10; i++)
      //{
      //  rows.Rows.Add(new R());
      //}

      //return View(rows);
    }

    private E GetEvent(Guid id)
    {
      return GetEvent(this.db.Events.OfType<E>(), id);
    }

    protected E GetEvent(IQueryable<E> context, Guid id)
    {
      IEnumerable<E> events = (from m in context where m.Id == id select m);
      if (events.Count() != 1)
      {
        throw new ApplicationException(string.Format("{0} events found with ID = {1}", events.Count(), id.ToString()));
      }

      return events.First();
    }

    private bool ParseMiles(string keyName, string milesInput, out int? newMiles)
    {
      bool success = true;
      newMiles = null;

      if (!string.IsNullOrEmpty(milesInput))
      {
        int rawMiles;
        if (!int.TryParse(milesInput, out rawMiles))
        {
          ModelState.SetModelValue(keyName, new ValueProviderResult(milesInput, milesInput, CultureInfo.CurrentUICulture));
          ModelState.AddModelError(keyName, "Must be a number");
          success = false;
        }
        newMiles = rawMiles;
      }

      return success;
    }

    private bool ParseDateValue(DateTime startDate, int numDays, Guid rowId, string inOrOut, FormCollection fields, out DateTime? newTime)
    {
      bool success = false;

      newTime = null;
      List<string> otherKeysWithText = new List<string>();

      for (int i = 0; i < numDays; i++)
      {
        string timeKey = string.Format("{0}{1:yyMMdd}_{2}", inOrOut, startDate.AddDays(i), rowId);
        string timeInput = fields[timeKey];
        if (!string.IsNullOrEmpty(timeInput))
        {
          otherKeysWithText.Add(timeKey);

          int maxTime = (inOrOut == "in") ? 2359 : 2400;
          int rawTime;
          if (int.TryParse(timeInput, out rawTime))
          {
            if (rawTime < 0 || rawTime > maxTime || (rawTime % 100) >= 60)
            {
              ModelState.SetModelValue(timeKey, new ValueProviderResult(timeInput, timeInput, CultureInfo.CurrentUICulture));
              ModelState.AddModelError(timeKey, "Use 0000-" + maxTime.ToString());
            }
            else if (newTime.HasValue)
            {
              foreach (string otherKey in otherKeysWithText)
              {
                ModelState.SetModelValue(otherKey, new ValueProviderResult(fields[otherKey], fields[otherKey], CultureInfo.CurrentUICulture));
                ModelState.AddModelError(otherKey, "Use 1 time " + inOrOut);
              }
            }
            else
            {
              newTime = startDate.AddDays(i) + new TimeSpan(rawTime / 100, rawTime % 100, 0);
              success = true;
            }
          }
          else
          {
            ModelState.SetModelValue(timeKey, new ValueProviderResult(timeInput, timeInput, CultureInfo.CurrentUICulture));
            ModelState.AddModelError(timeKey, "Use 0000-" + maxTime.ToString());
          }
        }
      }

      return success;
    }

    private ExpandedRowsContext GetRosterRows(E sarEvent)
    {
      throw new NotImplementedException();
      //DateTime goodBegin = sarEvent.StartTime.AddDays(-7);
      //DateTime goodEnd = sarEvent.StartTime.AddDays(14);

      //IEnumerable<EventRoster> goodRows = sarEvent.Roster.Where(mr => mr.TimeIn > goodBegin && mr.TimeIn < goodEnd && (!mr.TimeOut.HasValue || mr.TimeOut.Value < mr.TimeIn.Value.AddDays(10))).Cast<IRosterEntry>().OrderBy(f => f.TimeIn).OrderBy(f => f.Person.ReverseName);

      //IEnumerable<EventRoster> badRows = sarEvent.Roster.Where(mr => !(mr.TimeIn > goodBegin && mr.TimeIn < goodEnd && (!mr.TimeOut.HasValue || mr.TimeOut.Value < mr.TimeIn.Value.AddDays(10)))).Cast<IRosterEntry>();


      //DateTime earliest = sarEvent.StartTime.Date;
      //DateTime? maxTime = null;

      //if (goodRows.Count() > 0)
      //{
      //  DateTime tmp = goodRows.Min(x => x.TimeIn).Value.Date;
      //  if (sarEvent.StartTime > tmp)
      //  {
      //    earliest = tmp;
      //  }

      //  maxTime = goodRows.Max(x => x.TimeOut);
      //}

      //int numDays = maxTime.HasValue ? (maxTime.Value.Date - earliest).Days + 1 : 2;

      //return new ExpandedRowsContext
      //{
      //  Type = (typeof(E) == typeof(Mission)) ? RosterType.Mission : RosterType.Training,
      //  EventId = sarEvent.Id,
      //  NumDays = numDays,
      //  Rows = goodRows.OrderBy(f => (f.Person == null) ? "" : f.Person.ReverseName).ToList(),
      //  BadRows = badRows.ToList(),
      //  RosterStart = earliest.Date,
      //  SarEvent = sarEvent
      //};
    }

    public ActionResult ICS211(Guid id)
    {
      throw new NotImplementedException("reimplement");

      //var model = BuildRosterModel(id);
      //Dictionary<Guid, string> numbers = new Dictionary<Guid, string>();
      //var source = (model.SarEvent is Mission)
      //    ? this.db.MissionRosters.Where(f => f.Mission.Id == id).Select(f => f.Person.Id).Distinct()
      //    : this.db.TrainingRosters.Where(f => f.Training.Id == id).Select(f => f.Person.Id).Distinct();

      //foreach (var row in source)
      //{
      //  numbers.Add(row, this.db.PersonContact.Where(f => f.Person.Id == row && f.Subtype == "cell").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault());
      //}
      //ViewData["phones"] = numbers;
      //return View(model);
    }


    protected abstract bool Has4x4RosterPermissions();

    [HttpGet]
    public ActionResult Upload4x4Roster()
    {
      if (!Has4x4RosterPermissions()) return CreateLoginRedirect();

      return View();
    }

    [HttpPost]
    public ActionResult Review4x4Roster(bool? bot)
    {
      throw new NotImplementedException("reimplement");

      //if (!Has4x4RosterPermissions()) return CreateLoginRedirect();

      //// Value is pre-incremented in while loop.
      //int startRow = 13;

      //if (Request.Files.Count != 1)
      //{
      //  throw new InvalidOperationException("Can only submit one roster");
      //}

      //var postedFile = Request.Files[0];

      //byte[] fileData = new byte[postedFile.ContentLength];
      //postedFile.InputStream.Read(fileData, 0, fileData.Length);

      //var stream = new System.IO.MemoryStream(fileData);
      //stream.Position = 0;

      //ExcelFile xl = null; // new ExcelFile();
      //if (postedFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
      //{
      //  xl = ExcelService.Read(postedFile.InputStream, ExcelFileType.XLSX);
      //}
      //else
      //{
      //  xl = ExcelService.Read(postedFile.InputStream, ExcelFileType.XLS);
      //}
      //ExcelSheet sheet = xl.GetSheet(0);

      //object start = sheet.CellAt(13, 4).StringValue.ToUpperInvariant().Replace("DATE", "").Replace("TIME", "").Replace("IN", "").Replace("OUT", "").Trim();
      //DateTime startDate = DateTime.Parse(start.ToString());

      //ExpandedRowsContext model = new ExpandedRowsContext();
      //model.EventId = Guid.NewGuid();
      //if (typeof(E) == typeof(Mission))
      //{
      //  model.Type = RosterType.Mission;
      //  model.SarEvent = new Mission { Id = model.EventId };
      //}
      //else
      //{
      //  model.Type = RosterType.Training;
      //  model.SarEvent = new Training { Id = model.EventId };
      //}
      //model.SarEvent.StateNumber = sheet.CellAt(9, 2).StringValue.Split(' ')[0];
      //model.SarEvent.Title = sheet.CellAt(10, 2).StringValue;
      //model.SarEvent.Location = sheet.CellAt(11, 2).StringValue;
      //model.SarEvent.County = sheet.CellAt(9, 10).StringValue.ToLowerInvariant();

      //model.BadRows = new List<IRosterEntry>();
      //model.Rows = new List<IRosterEntry>();
      //model.NumDays = 3;
      //model.RosterStart = startDate;


      //SarUnit unit = this.db.Units.Where(f => f.DisplayName == "4x4").Single();
      ////ctx.Detach(unit);

      //while (!string.IsNullOrWhiteSpace((sheet.CellAt(++startRow, 1).StringValue ?? "").ToString()))
      //{
      //  R roster = new R();
      //  string name = sheet.CellAt(startRow, 1).StringValue;
      //  string dem = sheet.CellAt(startRow, 2).StringValue.PadLeft(4, '0');
      //  var member = this.db.Members
      //      .Where(f => f.LastName + ", " + f.FirstName == name && f.DEM == dem)
      //      .SingleOrDefault();

      //  if (member == null)
      //  {
      //    //throw new InvalidOperationException(string.Format("Member {0} not found", sheet.CellAt(startRow, 1]));
      //    continue;
      //  }

      //  //                    ctx.Detach(member);
      //  roster.Person = member;

      //  for (int i = 0; i < 3; i++)
      //  {
      //    string v = (sheet.CellAt(startRow, 8 - i * 2).StringValue ?? "").ToString();
      //    if (!string.IsNullOrWhiteSpace(v))
      //    {
      //      v = v.PadLeft(4, '0');
      //      int hours = int.Parse(v.Substring(0, 2));
      //      int minutes = int.Parse(v.Substring(2, 2));
      //      roster.TimeIn = startDate.AddDays(2 - i).AddHours(hours).AddMinutes(minutes);
      //    }
      //    v = (sheet.CellAt(startRow, 5 + i * 2).StringValue ?? "").ToString();
      //    if (!string.IsNullOrWhiteSpace(v))
      //    {
      //      v = v.PadLeft(4, '0');
      //      int hours = int.Parse(v.Substring(0, 2));
      //      int minutes = int.Parse(v.Substring(2, 2));
      //      roster.TimeOut = startDate.AddDays(i).AddHours(hours).AddMinutes(minutes);
      //    }
      //  }

      //  string miles = (sheet.CellAt(startRow, 11).StringValue ?? "").ToString();
      //  if (miles != "")
      //  {
      //    double numericMiles;
      //    if (double.TryParse(miles, out numericMiles))
      //    {
      //      roster.Miles = (int)Math.Ceiling(numericMiles);
      //    }
      //  }

      //  MissionRoster missionRoster = roster as MissionRoster;
      //  if (missionRoster != null)
      //  {
      //    missionRoster.Unit = unit;

      //    if ("CO".Equals(sheet.CellAt(startRow, 3).StringValue))
      //    {
      //      missionRoster.InternalRole = "InTown";
      //    }
      //    else if ("OL".Equals(sheet.CellAt(startRow, 3).StringValue))
      //    {
      //      missionRoster.InternalRole = "OL";
      //    }
      //    else if (new[] { "AS", "ATL", "OUA", "TL", "TLT", "TM" }.Contains(sheet.CellAt(startRow, 3).StringValue))
      //    {
      //      missionRoster.InternalRole = "Field";
      //    }
      //    else if ("VAN".Equals(sheet.CellAt(startRow, 3).StringValue))
      //    {
      //      missionRoster.InternalRole = "Base";
      //    }
      //    else
      //    {
      //      missionRoster.InternalRole = "Responder";
      //    }
      //  }
      //  model.Rows.Add(roster);
      //}

      //if (model.Rows.Count > 0)
      //{
      //  model.SarEvent.StartTime = model.Rows.Min(f => f.TimeIn).Value;
      //  model.SarEvent.StopTime = model.Rows.Max(f => f.TimeOut).Value;
      //}

      //DateTime early = model.SarEvent.StartTime.AddDays(-1);
      //DateTime late = model.SarEvent.StartTime.AddDays(1);
      //E[] alternates = this.GetEventSource().Where(f => f.StateNumber == model.SarEvent.StateNumber || (f.StartTime > early && f.StartTime < late)).OrderBy(f => f.StartTime).ToArray();

      //ViewData["alternateMissions"] = alternates;

      //ViewData["CanEditRoster"] = false;
      //ViewData["IsTemp"] = true;

      //SetSessionValue("reviewRosterBot", bot);
      //SetSessionValue("reviewRoster", model);
      //SetSessionValue("reviewRosterData", fileData);
      //SetSessionValue("reviewRosterName", postedFile.FileName);
      //return View(model);
    }

    [HttpPost]
    public ActionResult Submit4x4Roster(Guid missionId)
    {
      throw new NotImplementedException("reimplement");

      //if (!Has4x4RosterPermissions()) return CreateLoginRedirect();

      //ExpandedRowsContext model = (ExpandedRowsContext)GetSessionValue("reviewRoster");
      //if (model == null)
      //{
      //  throw new InvalidOperationException("Lost temporary roster data. Please start over.");
      //}

      //bool errors = false;
      //StringBuilder results = new StringBuilder();
      ////using (var ctx = GetContext())
      ////{
      //SarUnit unit = this.db.Units.Where(f => f.DisplayName == "4x4").Single();

      //if (model.EventId == missionId)
      //{
      //  AddEventToContext((E)model.SarEvent);
      //}
      //else
      //{
      //  model.SarEvent = GetEvent(missionId);
      //}

      //results.AppendLine(Url.Action("Roster", new { id = model.EventId }));

      //foreach (var row in model.Rows)
      //{
      //  AddRosterRowFrom4x4Sheet(model, unit, row);
      //}

      //try
      //{
      //  this.db.SaveChanges();

      //  byte[] fileData = (byte[])GetSessionValue("reviewRosterData");
      //  string filename = (string)GetSessionValue("reviewRosterName");

      //  Document doc = new Document
      //  {
      //    Size = fileData.Length,
      //    FileName = System.IO.Path.GetFileName(filename),
      //    Contents = fileData,
      //    ReferenceId = missionId,
      //    MimeType = Documents.GuessMime(filename),
      //    Type = "roster"
      //  };
      //  this.db.Documents.Add(doc);
      //  this.db.SaveChanges();
      //}
      //catch (DbEntityValidationException ex)
      //{
      //  errors = true;
      //  foreach (var entry in ex.EntityValidationErrors)
      //  {
      //    foreach (var err in entry.ValidationErrors)
      //    {
      //      var modelRow = model.Rows.SingleOrDefault(f => f.Id == ((IModelObject)entry.Entry.Entity).Id);
      //      results.AppendFormat("\"error\",\"{0}\",\"{1}\",\"{2}\",\"{3}\"\n",
      //        (modelRow == null) ? "" : modelRow.Person.ReverseName,
      //        (modelRow == null) ? err.PropertyName : modelRow.TimeIn.ToString(),
      //        (modelRow == null) ? entry.Entry.CurrentValues[err.PropertyName] : modelRow.TimeOut.ToString(),
      //        err.ErrorMessage);
      //    }
      //  }
      //}
      
      //bool? bot = (bool?)GetSessionValue("reviewRosterBot") ?? false;

      //return (bot.Value || errors) ?
      //    (ActionResult)(new ContentResult { Content = results.ToString(), ContentType = "text/plain" }) :
      //    new RedirectResult(Url.Action("Roster", new { id = missionId }));
    }

    protected abstract void AddRosterRowFrom4x4Sheet(ExpandedRowsContext model, UnitRow unit, EventRosterRow row);
  }

  public enum SarEventActions
  {
    CreateEvent,
    UpdateEvent,
    ReadEvent,
    DeleteEvent,
    AddRosterRow,
    UpdateRosterRow,
    DeleteRosterRow
  }

  public class CountSummaryRow
  {
    public string Key { get; set; }
    public int Count { get; set; }
  }

  public class RosterSummaryRow
  {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Missions { get; set; }
    public int Persons { get; set; }
    public double Hours { get; set; }
    public int Miles { get; set; }
  }
}
