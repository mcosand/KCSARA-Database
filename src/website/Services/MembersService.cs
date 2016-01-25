/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Models;
  using Newtonsoft.Json;
  /// <summary>
  /// 
  /// </summary>
  public interface IMembersService
  {
    MemberSummary GetMember(Guid id);
    object Contacts(Guid id);
    object Addresses(Guid id);

    object Memberships(Guid id);

    MemberMedical GetMedical(Guid id, bool showSensitive = false, string reason = "");

    IEnumerable<MemberSummary> Search(string query);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class MembersService : IMembersService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly Lazy<IEncryptionService> encryption;
    private readonly ICurrentPrincipalProvider principalProvider;
    private readonly ILog log;

    public MembersService(Func<IKcsarContext> dbFactory, Lazy<IEncryptionService> encryption, ICurrentPrincipalProvider principalProvider, ILog log)
    {
      this.dbFactory = dbFactory;
      this.encryption = encryption;
      this.principalProvider = principalProvider;
      this.log = log;
    }

    public MemberSummary GetMember(Guid id)
    {
      using (var db = dbFactory())
      {
        return SummariesWithUnits(db.Members.Where(f => f.Id == id)).FirstOrDefault();
      }
    }

    public object Contacts(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.PersonContact.Where(f => f.Person.Id == id)
          .Select(f => new
          {
            Value = f.Value,
            Type = f.Type,
            SubType = f.Subtype,
            Priority = f.Priority
          })
          .OrderBy(f => f.Type)
          .ThenBy(f => f.SubType)
          .ToArray();
      }
    }

    public object Addresses(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.PersonAddress.Where(f => f.Person.Id == id)
          .Select(f => new
          {
            f.Street,
            f.City,
            f.State,
            f.Zip,
            Type = f.InternalType.Trim(),
          })
          .OrderBy(f => f.Type)
          .ToArray();
      }
    }

    public IEnumerable<MemberSummary> Search(string query)
    {
      using (var db = dbFactory())
      {
        DateTime now = DateTime.Now;
        DateTime last12Months = now.AddMonths(-12);

        return
            SummariesWithUnits(
            db.Members.Where(f => (f.FirstName + " " + f.LastName).StartsWith(query)
                               || (f.LastName + ", " + f.FirstName).StartsWith(query)
                               || (f.DEM.StartsWith(query) || f.DEM.StartsWith("SR" + query)))
            .OrderByDescending(f => f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null || g.EndTime > now)))
            .ThenByDescending(f => f.MissionRosters.Count(g => g.TimeIn > last12Months))
            .ThenByDescending(f => f.MissionRosters.Count())
            .ThenBy(f => f.LastName)
            .ThenBy(f => f.FirstName)
            .ThenBy(f => f.Id))
            .ToArray();
      }
    }

    private IEnumerable<MemberSummary> SummariesWithUnits(IQueryable<MemberRow> query)
    {
      DateTime cutoff = DateTime.Now;
      return query
        .Select(f => new {
          Member = f,
          Units = f.Memberships
                    .Where(g => (g.EndTime == null || g.EndTime > cutoff) && g.Status.IsActive)
                    .Select(g => g.Unit)
                    .Select(g => new NameIdPair { Id = g.Id, Name = g.DisplayName }).Distinct()
        })
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

    public object Memberships(Guid id)
    {
      using (var db = dbFactory())
      {
        DateTime now = DateTime.Now;
        return db.Members.Where(f => f.Id == id)
          .SelectMany(f => f.Memberships)
          .Select(f => new
          {
            Id = f.Id,
            Unit = new NameIdPair { Id = f.Unit.Id, Name = f.Unit.DisplayName },
            Start = f.Activated,
            End = f.EndTime,
            Status = f.Status.StatusName,
            Current = f.Activated < now && (f.EndTime == null || f.EndTime > now),
            Active = f.Status.IsActive
          })
          .OrderBy(f => f.Unit.Name)
          .ThenByDescending(f => f.Start)
          .ToList();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">memberId</param>
    /// <param name="showSensitive"></param>
    /// <param name="description"></param>
    /// <param name="purpose"></param>
    /// <returns></returns>
    public MemberMedical GetMedical(Guid id, bool showSensitive = false, string reason = "")
    {
      bool isSelf = string.Equals(
        principalProvider.CurrentPrincipal.Claims.Where(f => f.Type == MemberIdClaimsTransformer.Type).Select(f => f.Value).FirstOrDefault(),
        id.ToString(),
        StringComparison.OrdinalIgnoreCase);
      ////if (!(Permissions.IsUser || Permissions.IsSelf(id))) ThrowAuthError();

      if (!isSelf) throw new AccessDeniedException();

      using (var db = dbFactory())
      {
        var data = db.Members.Where(f => f.Id == id).Select(f => f.MedicalInfo).SingleOrDefault();
        var contacts = db.Members.Where(f => f.Id == id).SelectMany(f => f.EmergencyContacts).ToArray();

        //if (showSensitive)
        //{
        //  if (!Permissions.IsSelf(id))
        //  {
        //    if (string.IsNullOrWhiteSpace(reason)) ThrowSubmitErrors(new[] { new Web.Model.SubmitError { Error = "Reason not specified", Property = "reason" } });

        //    Model.SensitiveInfoAccess infoAccess = new Model.SensitiveInfoAccess
        //    {
        //      Owner = db.Members.Single(f => f.Id == id),
        //      Action = "Read Medical Information",
        //      Actor = (Permissions.UserId == Guid.Empty) ? User.Identity.Name : db.Members.Single(f => f.Id == Permissions.UserId).FullName,
        //      Reason = reason,
        //      Timestamp = DateTime.Now,
        //    };
        //    db.SensitiveInfoLog.Add(infoAccess);
        //    db.SaveChanges();
        //  }
        //}

        return new MemberMedical
        {
          IsSensitive = showSensitive,
          Allergies = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedAllergies),
          Medications = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedMedications),
          Disclosure = data == null ? null : HiddenOrDecrypted(showSensitive, data.EncryptedDisclosures),
          Contacts = contacts.Select(f =>
          {
            if (showSensitive)
            {
              var contact = JsonConvert.DeserializeObject<EmergencyContactData>(encryption.Value.Decrypt(f.EncryptedData));
              return new EmergencyContact
              {
                IsSensitive = true,
                Name = contact.Name,
                Relation = contact.Relation,
                Type = contact.Type,
                Number = contact.Number,
                Id = f.Id
              };
            }
            else
            {
              return new EmergencyContact
              {
                IsSensitive = false,
                Name = Strings.SensitiveText,
                Type = null,
              };
            }
          })
        };
      }
    }

    private string HiddenOrDecrypted(bool decrypt, string value)
    {
      return decrypt
          ? ((value == null) ? null : encryption.Value.Decrypt(value))
          : Strings.SensitiveText;
    }
  }
}
