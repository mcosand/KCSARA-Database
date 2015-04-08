/*
 * Copyright 2012-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using Data = Kcsar.Database.Data;
  using Kcsara.Database.Web.Model;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Web.Http;
  using log4net;
  using Kcsara.Database.Web.api.Models;
  using Kcsar.Database.Model;

  [ModelValidationFilter]
  public class TrainingRecordsController : BaseApiController
  {
    public TrainingRecordsController(Data.IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpGet]
    public TrainingRecord Get(Guid id)
    {
      return GetObjectOrNotFound(
          () => GetTrainingRecords(f => f.Id == id, true, null).SingleOrDefault()
      );
    }

    [HttpGet]
    public IEnumerable<TrainingRecord> FindComputedForMember(Guid id)
    {
      if (!User.IsInRole("cdb.users")) ThrowAuthError();

      Data.MemberRow m = GetObjectOrNotFound(() => db.Members.SingleOrDefault(f => f.Id == id));

      var model = GetComputedTrainingRecordViews(f => f.Member.Id == id, false, m.WacLevel);

      return model;
    }

    private IEnumerable<TrainingRecord> GetComputedTrainingRecordViews(Expression<Func<Data.ComputedTrainingRecordRow, bool>> whereClause, bool includeMember, WacLevel? levelForRequired)
    {
      //int mask = (1 << (((int)(levelForRequired ?? Data.WacLevel.None) - 1) * 2 + 1));
      //string dateFormat = GetDateFormat();

      //var model = db.ComputedTrainingAwards.Where(whereClause).Select(computed =>
      //                             new
      //                             {
      //                               Course = new TrainingCourse
      //                               {
      //                                 Id = computed.Course.Id,
      //                                 Title = computed.Course.DisplayName,
      //                                 Required = computed.Course.WacRequired
      //                               },
      //                               Comments = (from award in db.TrainingAward
      //                                           where award.Completed == computed.Completed && award.Course.Id == computed.Course.Id && award.Member.Id == computed.Member.Id
      //                                           select award.metadata).FirstOrDefault(),
      //                               Completed = computed.Completed,
      //                               Expires = computed.Expiry,
      //                               Source = (computed.Rule != null) ? "rule" : (computed.Roster != null) ? "roster" : "direct",
      //                               ReferenceId = ((Guid?)computed.Rule.Id) ??
      //                                           (from roster in db.TrainingRosters where roster.Id == computed.Roster.Id select (Guid?)roster.Training.Id).FirstOrDefault<Guid?>() ??
      //                                           (from award in db.TrainingAward
      //                                            where award.Completed == computed.Completed && award.Course.Id == computed.Course.Id && award.Member.Id == computed.Member.Id
      //                                            select award.Id).FirstOrDefault(),
      //                               Required = (levelForRequired == null) ? (bool?)null : (computed.Course.WacRequired & mask) > 0
      //                             }).OrderByDescending(f => f.Completed).ThenBy(f => f.Source)
      //                             .AsEnumerable().Select(f => new TrainingRecord
      //                                 {
      //                                   Course = f.Course,
      //                                   Comments = f.Comments,
      //                                   Completed = string.Format(dateFormat, f.Completed),
      //                                   Expires = string.Format(dateFormat, f.Expires),
      //                                   Source = f.Source,
      //                                   ReferenceId = f.ReferenceId,
      //                                   Required = f.Required
      //                                 });
      //return model;
      throw new NotImplementedException("reimplement");

    }

    private IEnumerable<TrainingRecord> GetTrainingRecords(Expression<Func<Data.TrainingRecordRow, bool>> whereClause, bool includeMember, WacLevel? levelForRequired)
    {
      throw new NotImplementedException("reimplement");

      //int mask = (1 << (((int)(levelForRequired ?? Data.WacLevel.None) - 1) * 2 + 1));
      //string dateFormat = GetDateFormat();

      //var model = db.TrainingAward.Where(whereClause).Select(
      //            award => new
      //            {
      //              Course = new TrainingCourse
      //              {
      //                Id = award.Course.Id,
      //                Title = award.Course.DisplayName
      //              },
      //              Comments = award.metadata,
      //              Completed = award.Completed,
      //              Expires = award.Expiry,
      //              Source = (award.Roster != null) ? "roster" : "direct",
      //              ReferenceId = (from roster in db.TrainingRosters where roster.Id == award.Roster.Id select (Guid?)roster.Training.Id).FirstOrDefault<Guid?>() ??
      //                              award.Id,
      //              Required = (award.Course.WacRequired & mask) > 0
      //            }).AsEnumerable().Select(f => new TrainingRecord
      //            {
      //              Course = f.Course,
      //              Comments = f.Comments,
      //              Completed = string.Format(dateFormat, f.Completed),
      //              Expires = string.Format(dateFormat, f.Expires),
      //              Source = f.Source,
      //              ReferenceId = f.ReferenceId,
      //              Required = f.Required
      //            }).OrderByDescending(f => f.Completed).ThenBy(f => f.Source).ToArray();
      //return model;
    }

    [HttpGet]
    public IEnumerable<TrainingRecord> FindForMember(Guid id)
    {
      if (!User.IsInRole("cdb.users")) ThrowAuthError();

      Data.MemberRow m = GetObjectOrNotFound(() => db.Members.SingleOrDefault(f => f.Id == id));

      int mask = (1 << (((int)m.WacLevel - 1) * 2 + 1));

      var model = GetTrainingRecords(f => f.Member.Id == id, false, m.WacLevel);

      return model;
    }

    // POST api/<controller>
    [HttpPost]
    public TrainingRecord Post([FromBody]TrainingRecord view)
    {
      log.DebugFormat("POST {0} {1} {2}", Request.RequestUri, User.Identity.Name, JsonConvert.SerializeObject(view));
      if (!User.IsInRole("cdb.trainingeditors")) ThrowAuthError();
      //     if (!Permissions.IsAdmin && !Permissions.IsSelf(view.MemberId) && !Permissions.IsMembershipForPerson(view.MemberId)) return GetAjaxLoginError<MemberContactView>(null);
      List<SubmitError> errors = new List<SubmitError>();

      if (view.Member.Id == Guid.Empty)
      {
        ThrowSubmitErrors(new[] { new SubmitError { Error = Strings.API_Required, Property = "Member" } });
      }

      Data.TrainingRecordRow model;
      model = (from a in db.TrainingRecords.Include("Member").Include("Course") where a.Id == view.ReferenceId select a).FirstOrDefault();

      if (model == null)
      {
        model = new Data.TrainingRecordRow();
        model.Member = db.Members.Where(f => f.Id == view.Member.Id).FirstOrDefault();
        if (model.Member == null)
        {
          errors.Add(new SubmitError { Property = "Member", Error = Strings.API_NotFound });
          ThrowSubmitErrors(errors);
        }
        db.TrainingRecords.Add(model);
      }

 //     try
 //     {
        model.UploadsPending = (view.PendingUploads > 0);

        DateTime completed;
        if (string.IsNullOrWhiteSpace(view.Completed))
        {
          errors.Add(new SubmitError { Property = "Completed", Error = Strings.API_Required });
        }
        else if (!DateTime.TryParse(view.Completed, out completed))
        {
          errors.Add(new SubmitError { Error = Strings.API_InvalidDate, Property = "Completed" });
        }
        else
        {
          model.Completed = completed;
        }

        if (model.metadata != view.Comments) model.metadata = view.Comments;

        if (model.Member.Id != view.Member.Id)
        {
          throw new InvalidOperationException("Don't know how to change member yet");
        }

        if (view.Course.Id == null)
        {
          errors.Add(new SubmitError { Error = Strings.API_Required, Id = new[] { view.ReferenceId }, Property = "Course" });
        }
        else if (model.Course == null || model.Course.Id != view.Course.Id)
        {
          model.Course = (from c in db.TrainingCourses where c.Id == view.Course.Id select c).First();
        }

        switch (view.ExpirySrc)
        {
          case "default":
            model.Expiry = model.Course.ValidMonths.HasValue ? model.Completed.AddMonths(model.Course.ValidMonths.Value) : (DateTime?)null;
            break;
          case "custom":
            if (!string.IsNullOrWhiteSpace(view.Expires))
            {
              model.Expiry = DateTime.Parse(view.Expires);
            }
            else
            {
              errors.Add(new SubmitError { Error = Strings.API_TrainingRecord_CustomExpiryRequired, Property = "Expiry", Id = new[] { view.ReferenceId } });
            }
            break;
          case "never":
            model.Expiry = null;
            break;
        }

        // Documentation required.
        if (!model.UploadsPending && string.IsNullOrWhiteSpace(model.metadata))
        {
          errors.Add(new SubmitError { Error = Strings.API_TrainingRecord_DocumentationRequired, Property = BaseApiController.ModelRootNodeName });
        }

        // Prevent duplicate records
        if (db.TrainingRecords.Where(f => f.Id != model.Id && f.Completed == model.Completed && f.Course.Id == model.Course.Id && f.Member.Id == model.Member.Id).Count() > 0)
        {
          ThrowSubmitErrors(new[] { new SubmitError { Error = Strings.API_TrainingRecord_Duplicate, Id = new[] { model.Id }, Property = BaseApiController.ModelRootNodeName } });
        }

        if (errors.Count == 0)
        {
          db.SaveChanges();
          db.RecalculateTrainingAwards(model.Member.Id);
          db.SaveChanges();
        }

        view.ReferenceId = model.Id;
        view.Course.Title = model.Course.DisplayName;
      //}
      //catch (RuleViolationsException ex)
      //{
      //  foreach (RuleViolation v in ex.Errors)
      //  {
      //    errors.Add(new SubmitError { Error = v.ErrorMessage, Property = v.PropertyName, Id = new[] { v.EntityKey } });
      //  }
      //  // throw new InvalidOperationException("TODO: report errors");
      //}

      //if (errors.Count > 0)
      //{
      //  ThrowSubmitErrors(errors);
      //}

      return view;
    }

    [HttpPost]
    public void Delete(Guid id)
    {
      if (!User.IsInRole("cdb.trainingeditors")) ThrowAuthError();

      List<string> files = new List<string>();

      var item = GetObjectOrNotFound(() => db.TrainingRecords.FirstOrDefault(f => f.Id == id));
      var memberId = item.Member.Id;

      db.TrainingRecords.Remove(item);
      db.SaveChanges();
      db.RecalculateTrainingAwards(memberId);
      db.SaveChanges();
    }
  }
}
