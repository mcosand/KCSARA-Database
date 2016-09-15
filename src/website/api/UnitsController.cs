/*
 * Copyright 2013-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using System.Web.Http;
  using Kcsara.Database.Web.Services;
  using Kcsara.Database.Web.api.Models;
  using log4net;
  using Model = Kcsar.Database.Model;

  [ModelValidationFilter]
  public class UnitsController : BaseApiController
  {
    readonly Lazy<MembersController> membersController;

    public UnitsController(Lazy<MembersController> membersController, Services.IAuthService auth, Model.IKcsarContext db, ILog log)
      : base(db, auth, log)
    {
      this.membersController = membersController;
    }

    #region Applications
    [HttpPost]
    public bool SubmitApplication(Guid id, Guid memberId)
    {
      if (!CanEditApplication(Permissions, memberId, id)) ThrowAuthError();

      RegisterApplication(db, id, db.Members.Single(f => f.Id == memberId));
      db.SaveChanges();

      return true;
    }

    [HttpPost]
    public bool WithdrawApplication(Guid id)
    {
      try
      {
        Model.UnitApplicant application = db.UnitApplicants.Include("Applicant", "Unit").Single(f => f.Id == id);

        if (!CanEditApplication(Permissions, application.Applicant.Id, application.Unit.Id)) ThrowAuthError();

        db.UnitApplicants.Remove(application);

        db.SaveChanges();

        // $TODO - if appropriate, clean up member?
        return true;
      }
      catch (HttpResponseException)
      {
        throw;
      }
      catch (Exception ex)
      {
        log.Error(ex);
      }
      return false;
    }

    internal static void RegisterApplication(Model.IKcsarContext db, Guid id, Model.Member member)
    {
      Model.UnitApplicant application = new Model.UnitApplicant
      {
        Unit = db.Units.Single(f => f.Id == id),
        Applicant = member,
        Started = DateTime.Now,
        IsActive = true,
      };
      db.UnitApplicants.Add(application);
    }

    internal static bool CanEditApplication(IAuthService perms, Guid memberId, Guid unitId)
    {
      return perms.IsAdmin || perms.IsSelf(memberId) || perms.IsRoleForUnit("applications", unitId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Unit id</param>
    /// <returns></returns>
    [HttpGet]
    public UnitApplicant[] GetApplicants(Guid id)
    {
      if (!Permissions.IsUser) ThrowAuthError();

      Guid rootOrgId = Guid.Empty;
      Guid.TryParse(ConfigurationManager.AppSettings["rootOrgId"], out rootOrgId);

      var query = db.UnitApplicants.Include("Applicant.ContactNumbers", "Applicant.EmergencyContacts");
      if (id != rootOrgId)
      {
        query = query.Where(f => f.Unit.Id == id);
      }

      MembersController members = membersController.Value;

      var notDoneDocs = new[] { Model.DocumentStatus.NotApplicable.ToString(), Model.DocumentStatus.NotStarted.ToString() };

      return query.OrderBy(f => f.Applicant.LastName).ThenBy(f => f.Applicant.FirstName).AsEnumerable().Select(f =>
      {
        var docs = members.GetUnitDocuments(f.Applicant.Id);

        return new UnitApplicant
    {
      Id = f.Id,
      MemberId = f.Applicant.Id,
      NameReverse = f.Applicant.ReverseName,
      Email = f.Applicant.ContactNumbers.Where(g => g.Type == "email").OrderBy(g => g.Priority).Select(g => g.Value).FirstOrDefault(),
      EmergencyContactCount = f.Applicant.EmergencyContacts.Count,
      Background = f.Applicant.BackgroundText,
      Username = f.Applicant.Username,
      Started = f.Started,
      Active = f.IsActive,
      RemainingDocCount = docs.Count() - docs.Count(g => !notDoneDocs.Contains(g.Status))
    };
      }).ToArray();
    }
    #endregion

    #region Unit Documents
    [HttpGet]
    public UnitDocument[] GetDocuments(Guid id)
    {
      if (!(Permissions.IsUser || Permissions.IsSelf(id))) ThrowAuthError();

      var result = db.UnitDocuments
          .Where(f => f.Unit.Id == id)
          .OrderBy(f => f.Order).ThenBy(f => f.Title)
          .Select(UnitDocument.UnitDocumentConversion);
      return result.ToArray();
    }

    [HttpPost]
    public string SaveDocuments(Guid id, UnitDocument[] data)
    {
      if (!CanEditDocuments(Permissions, id))
        ThrowAuthError();

      Model.SarUnit unit = db.Units.Include("Documents").Single(f => f.Id == id);

      var existingDocuments = db.UnitDocuments.Where(f => f.Unit.Id == id).ToDictionary(f => f.Id, f => f);

      List<UnitDocument> desiredDocuments = new List<UnitDocument>(data);
      foreach (var document in desiredDocuments)
      {
        Model.UnitDocument unitDocument;
        if (existingDocuments.TryGetValue(document.Id, out unitDocument))
        {
          existingDocuments.Remove(document.Id);
        }

        if (string.IsNullOrWhiteSpace(document.Title))
        {
          // If there's no name, delete it.
          if (unitDocument != null)
          {
            unit.Documents.Remove(unitDocument);
          }
          continue;
        }

        if (string.IsNullOrWhiteSpace(document.Url))
          return string.Format("{0}'s url is blank", document.Title);

        if (unitDocument == null)
        {
          unitDocument = new Model.UnitDocument()
          {
            Type = Model.UnitDocumentType.Application
          };
          unit.Documents.Add(unitDocument);
        }

        document.UpdateModel(unitDocument);
      }

      foreach (var leftover in existingDocuments.Values)
      {
        unit.Documents.Remove(leftover);
      }

      db.SaveChanges();

      return "OK";
    }

    internal static bool CanEditDocuments(IAuthService perms, Guid unitId)
    {
      return perms.IsAdmin || perms.IsRoleForUnit("documents", unitId);
    }


    #endregion

  }
}
