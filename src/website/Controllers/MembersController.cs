/*
 * Copyright 2013-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using System.Text.RegularExpressions;
  //using System.Web.Http;
  using Database.Services;
  //using Kcsara.Database.Web.api.Models;
  //using Kcsara.Database.Web.Services;
  using log4net;
  using Microsoft.AspNet.Mvc;
  using Newtonsoft.Json;
  using ObjectModel.Accounts;
  using ViewModels;
  using Model = Kcsar.Database.Model;

  public class MembersController : BaseController
  {
    readonly Lazy<AccountsService> accounts;

    public MembersController(Lazy<AccountsService> accounts, Lazy<Model.IKcsarContext> db, ILog log)
      : base(db, log)
    {
      this.accounts = accounts;
    }

    /// <summary>Gets account information for a given member.</summary>
    /// <remarks>used by /account/detail/{username}</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public AccountInfo AccountFor(Guid id)
    {
      var username = dbFactory.Value.Members.Where(f => f.Id == id).Select(f => f.Username).FirstOrDefault();
      if (string.IsNullOrWhiteSpace(username))
      {
        return null;
      }

      return accounts.Value.Get(username);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Member id</param>
    /// <returns></returns>
    //[HttpGet]
    //public MembersApplication[] GetApplications(Guid id)
    //{
    //  if (!(Permissions.IsUser || Permissions.IsSelf(id))) ThrowAuthError();
    //  var result = db.Members.Where(f => f.Id == id).SelectMany(f => f.ApplyingTo)
    //      .Select(f => new MembersApplication
    //      {
    //        Id = f.Id,
    //        Unit = new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
    //        IsActive = f.IsActive,
    //        Started = f.Started
    //      }).ToArray();

    //  foreach (var item in result)
    //  {
    //    item.CanEdit = UnitsController.CanEditApplication(Permissions, id, item.Unit.Id);
    //  }

    //  return result;
    //}

    //[HttpGet]
    //public MemberUnitDocument[] GetUnitDocuments(Guid id)
    //{
    //  if (!(Permissions.IsUser || Permissions.IsSelf(id))) ThrowAuthError();

    //  Dictionary<Guid, MemberUnitDocument> result = new Dictionary<Guid, MemberUnitDocument>();

    //  var applyingToUnits = this.GetApplications(id).Select(f => f.Unit.Id).ToList();

    //  string rootOrgId = ConfigurationManager.AppSettings["rootOrgId"];
    //  if (!string.IsNullOrWhiteSpace(rootOrgId) && applyingToUnits.Count > 0) applyingToUnits.Add(new Guid(rootOrgId));

    //  var member = db.Members.Single(f => f.Id == id);
    //  int? yearsOldNullable = (DateTime.Now - member.BirthDate).Years();
    //  bool knowAge = yearsOldNullable.HasValue;
    //  int yearsOld = yearsOldNullable ?? 0;

    //  var appDocs = db.Units
    //      .Where(f => applyingToUnits.Contains(f.Id))
    //      .SelectMany(f => f.Documents.Where(g => (
    //          (g.Type & Model.UnitDocumentType.Application) == Model.UnitDocumentType.Application)
    //          && (g.ForMembersYounger == null || !knowAge || g.ForMembersYounger > yearsOld)
    //          && (g.ForMembersOlder == null || !knowAge || g.ForMembersOlder <= yearsOld)))
    //      .Select(MemberUnitDocument.UnitDocumentConversion);

    //  foreach (var doc in appDocs)
    //  {
    //    if (!result.ContainsKey(doc.DocId)) result.Add(doc.DocId, doc);
    //  }

    //  foreach (var memberDoc in db.Members.Include("UnitDocuments.Unit")
    //      .Where(f => f.Id == id)
    //      .SelectMany(f => f.UnitDocuments)
    //      .Select(MemberUnitDocument.MemberUnitDocumentConversion))
    //  {
    //    if (result.ContainsKey(memberDoc.DocId))
    //    {
    //      result[memberDoc.DocId] = memberDoc;
    //    }
    //    else
    //    {
    //      result.Add(memberDoc.DocId, memberDoc);
    //    }
    //  }

    //  return result.Values.OrderBy(f => f.Unit.Name).ThenBy(f => f.Title).Select(f => FilterUnitDocActions(id, f)).ToArray();
    //}

    //private MemberUnitDocument FilterUnitDocActions(Guid memberId, MemberUnitDocument doc)
    //{
    //  var self = this.Permissions.IsSelf(memberId);
    //  if (!self) doc.Actions = doc.Actions & ~MemberUnitDocumentActions.UserReview;
    //  if (!this.Permissions.IsAdmin)
    //  {
    //    if (!this.Permissions.IsRoleForUnit("applicants", doc.Unit.Id) && !this.Permissions.IsMembershipForUnit(doc.Unit.Id))
    //    {
    //      // If not management, remove the unit-level links
    //      doc.Actions = doc.Actions & ~(MemberUnitDocumentActions.Complete | MemberUnitDocumentActions.Reject);

    //      // If not management and not editing yourself, remove the 'submit' action.
    //      if (!self) doc.Actions = doc.Actions & ~MemberUnitDocumentActions.Submit;
    //    }
    //  }
    //  return doc;
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="id">member id</param>
    ///// <param name="docId"></param>
    ///// <returns></returns>
    //[HttpPost]
    //public bool SignUnitDocument(Guid id, Guid docId, Guid? mDocId = null)
    //{
    //  return _SetUnitDocumentState(id, docId, mDocId, MemberUnitDocumentActions.UserReview, Model.DocumentStatus.Complete, Permissions.IsSelf(id));
    //}

    //[HttpPost]
    //public bool SubmitUnitDocument(Guid id, Guid docId, Guid? mDocId = null)
    //{
    //  return _SetUnitDocumentState(id, docId, mDocId, MemberUnitDocumentActions.Submit, Model.DocumentStatus.Submitted, Permissions.IsSelf(id));
    //}

    //[HttpPost]
    //public bool SignoffUnitDocument(Guid id, Guid docId, bool approve, Guid? mDocId = null)
    //{
    //  return _SetUnitDocumentState(
    //      id,
    //      docId,
    //      mDocId,
    //      approve ? MemberUnitDocumentActions.Complete : MemberUnitDocumentActions.Reject,
    //      approve ? Model.DocumentStatus.Complete : Model.DocumentStatus.NotStarted,
    //      false);
    //}

    //private bool _SetUnitDocumentState(Guid id, Guid docId, Guid? mDocId, MemberUnitDocumentActions allowedActions, Model.DocumentStatus targetState, bool fromUser)
    //{
    //  MemberUnitDocument doc = null;
    //  Model.MemberUnitDocument modelDoc = null;
    //  if (mDocId.HasValue)
    //  {
    //    modelDoc = db.MemberUnitDocuments.Include("Document.Unit", "Member").Single(f => f.Id == mDocId);
    //    doc = MemberUnitDocument.MemberUnitDocumentConversion.Compile()(modelDoc);
    //  }
    //  else
    //  {
    //    var unitDoc = db.UnitDocuments.Include("Unit").Single(f => f.Id == docId);
    //    var member = db.Members.Single(f => f.Id == id);
    //    modelDoc = new Model.MemberUnitDocument
    //    {
    //      Member = member,
    //      Document = unitDoc,
    //      Status = Model.DocumentStatus.NotStarted,
    //    };
    //    member.UnitDocuments.Add(modelDoc);
    //    doc = db.UnitDocuments.Where(f => f.Id == docId).Select(MemberUnitDocument.UnitDocumentConversion).Single();
    //  }

    //  FilterUnitDocActions(id, doc);

    //  if ((doc.Actions & allowedActions) == MemberUnitDocumentActions.None) ThrowAuthError();

    //  modelDoc.Status = targetState;
    //  if (fromUser)
    //  {
    //    modelDoc.UnitAction = DateTime.Now;
    //  }
    //  else
    //  {
    //    modelDoc.MemberAction = DateTime.Now;
    //  }

    //  db.SaveChanges();

    //  return true;
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="id">memberId</param>
    ///// <param name="showSensitive"></param>
    ///// <param name="description"></param>
    ///// <param name="purpose"></param>
    ///// <returns></returns>
    //[HttpGet]
    //public MemberMedical GetMedical(Guid id, bool showSensitive = false, string reason = "")
    //{
    //  if (!(Permissions.IsUser || Permissions.IsSelf(id))) ThrowAuthError();

    //  var data = db.Members.Where(f => f.Id == id).Select(f => f.MedicalInfo).SingleOrDefault();
    //  var contacts = db.Members.Where(f => f.Id == id).SelectMany(f => f.EmergencyContacts).ToArray();

    //  if (showSensitive)
    //  {
    //    if (!Permissions.IsSelf(id))
    //    {
    //      if (string.IsNullOrWhiteSpace(reason)) ThrowSubmitErrors(new[] { new Web.Model.SubmitError { Error = "Reason not specified", Property = "reason" } });

    //      Model.SensitiveInfoAccess infoAccess = new Model.SensitiveInfoAccess
    //      {
    //        Owner = db.Members.Single(f => f.Id == id),
    //        Action = "Read Medical Information",
    //        Actor = (Permissions.UserId == Guid.Empty) ? User.Identity.Name : db.Members.Single(f => f.Id == Permissions.UserId).FullName,
    //        Reason = reason,
    //        Timestamp = DateTime.Now,
    //      };
    //      db.SensitiveInfoLog.Add(infoAccess);
    //      db.SaveChanges();
    //    }
    //  }

    //  return new MemberMedical
    //  {
    //    IsSensitive = showSensitive,
    //    Allergies = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedAllergies),
    //    Medications = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedMedications),
    //    Disclosure = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedDisclosures),
    //    Contacts = contacts.Select(f =>
    //    {
    //      if (showSensitive)
    //      {
    //        var contact = JsonConvert.DeserializeObject<Kcsar.Database.Model.EmergencyContactData>(EncryptionService.Unprotect(EncryptionService.MEMBER_SENSITIVE, f.EncryptedData));
    //        return new EmergencyContact
    //        {
    //          IsSensitive = true,
    //          Name = contact.Name,
    //          Relation = contact.Relation,
    //          Type = contact.Type,
    //          Number = contact.Number,
    //          Id = f.Id
    //        };
    //      }
    //      else
    //      {
    //        return new EmergencyContact
    //        {
    //          IsSensitive = false,
    //          Name = Strings.SensitiveText,
    //          Type = null,
    //        };
    //      }
    //    })
    //  };
    //}

    //private string HiddenOrDecrypted(bool decrypt, string value)
    //{
    //  return decrypt
    //      ? ((value == null) ? null : EncryptionService.Unprotect(EncryptionService.MEMBER_SENSITIVE, value))
    //      : Strings.SensitiveText;
    //}

    //[HttpPost]
    //public string SaveMedical(MemberMedical data)
    //{
    //  if (data == null || data.Member == null || data.Member.Id == Guid.Empty)
    //    ThrowSubmitErrors(new[] { new Web.Model.SubmitError { Error = "No user specified" } });

    //  if (!(Permissions.IsAdmin || Permissions.IsMembershipForPerson(data.Member.Id) || Permissions.IsSelf(data.Member.Id)))
    //    ThrowAuthError();


    //  Model.Member member = db.Members.Include("MedicalInfo", "EmergencyContacts").Single(f => f.Id == data.Member.Id);
    //  Model.MemberMedical medical = member.MedicalInfo;
    //  if (medical == null)
    //  {
    //    medical = new Model.MemberMedical();
    //    member.MedicalInfo = medical;
    //  }

    //  medical.EncryptedAllergies = string.IsNullOrWhiteSpace(data.Allergies) ? null : EncryptionService.Protect(EncryptionService.MEMBER_SENSITIVE, data.Allergies);
    //  medical.EncryptedMedications = string.IsNullOrWhiteSpace(data.Medications) ? null : EncryptionService.Protect(EncryptionService.MEMBER_SENSITIVE, data.Medications);
    //  medical.EncryptedDisclosures = string.IsNullOrWhiteSpace(data.Disclosure) ? null : EncryptionService.Protect(EncryptionService.MEMBER_SENSITIVE, data.Disclosure);

    //  var existingContacts = db.Members.Where(f => f.Id == data.Member.Id).SelectMany(f => f.EmergencyContacts).ToDictionary(f => f.Id, f => f);

    //  List<EmergencyContact> desiredContacts = new List<EmergencyContact>(data.Contacts);
    //  foreach (var contact in desiredContacts)
    //  {
    //    var cData = new Model.EmergencyContactData
    //    {
    //      Name = contact.Name,
    //      Relation = contact.Relation,
    //      Type = contact.Type,
    //      Number = contact.Number
    //    };

    //    Model.MemberEmergencyContact memberContact;
    //    if (existingContacts.TryGetValue(contact.Id, out memberContact))
    //    {
    //      existingContacts.Remove(contact.Id);
    //    }

    //    if (string.IsNullOrWhiteSpace(contact.Name))
    //    {
    //      // If there's no name, delete it.
    //      if (memberContact != null)
    //      {
    //        member.EmergencyContacts.Remove(memberContact);
    //      }
    //      continue;
    //    }

    //    if (string.IsNullOrWhiteSpace(contact.Number))
    //      return string.Format("{0}'s number is blank", contact.Name);

    //    if (memberContact == null)
    //    {
    //      memberContact = new Model.MemberEmergencyContact();
    //      member.EmergencyContacts.Add(memberContact);
    //    }

    //    memberContact.EncryptedData = EncryptionService.Protect(EncryptionService.MEMBER_SENSITIVE, JsonConvert.SerializeObject(cData));
    //  }

    //  foreach (var leftover in existingContacts.Values)
    //  {
    //    member.EmergencyContacts.Remove(leftover);
    //  }
    //  db.SaveChanges();

    //  return "OK";
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<MemberSummary> ByWorkerNumber(string id)
    {
      id = id.TrimStart('S', 'R');

      var result = dbFactory.Value.Members.Where(f => f.DEM == id || f.DEM == "SR" + id)
        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName)
        .AsEnumerable()
        .Select(f => new MemberSummary
        {
          Name = f.FullName,
          WorkerNumber = f.DEM,
          Id = f.Id
        });

      return result;
    }

    [HttpGet]
    public IEnumerable<MemberSummary> ByPhoneNumber(string id)
    {
      if (id.Length < 10 || !Regex.IsMatch(id, "\\d+"))
      {
        return new MemberSummary[0];
      }

      var pattern = string.Format("%{0}%{1}%{2}%",
        id.Substring(id.Length - 10, 3),
        id.Substring(id.Length - 7, 3),
        id.Substring(id.Length - 4, 4));

      var result = dbFactory.Value.Members.Where(f => f.ContactNumbers.Any(g => SqlFunctions.PatIndex(pattern, g.Value) > 0))
        .AsEnumerable()
        .Select(f => new MemberSummary
        {
          Name = f.FullName,
          WorkerNumber = f.DEM,
          Id = f.Id
        });

      return result;
    }
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>used by /account/detail/{username}, MissionLine app</remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<MemberSummary> ByUsername(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
      }

      var result = SummariesWithUnits(db.Members.Where(f => f.Username == id)
        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName));

      return result;
    }
    */
    internal static IEnumerable<MemberSummary> SummariesWithUnits(IQueryable<Model.Member> query)
    {
      DateTime cutoff = DateTime.Now;
      return query.Select(f => new { Member = f, Units = f.Memberships.Where(g => (g.EndTime == null || g.EndTime > cutoff) && g.Status.IsActive).Select(g => g.Unit).Select(g => new NameIdPair { Id = g.Id, Name = g.DisplayName }).Distinct() })
        .AsEnumerable()
        .Select(f => new MemberSummary
        {
          Name = f.Member.FullName,
          WorkerNumber = f.Member.DEM,
          Id = f.Member.Id,
          Units = f.Units.ToArray(),
          Photo = f.Member.PhotoFile
        });
    }
  }
}