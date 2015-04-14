/*
 * Copyright 2013-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Infrastructure;
  using System.Linq;
  using Kcsar.Database.Data.Events;

  public interface IKcsarContext : IDisposable
  {
    IDbSet<AnimalRow> Animals { get; set; }
    IDbSet<AnimalEventRow> AnimalMissions { get; set; }
    IDbSet<AnimalOwnerRow> AnimalOwners { get; set; }
    IDbSet<SarEventRow> Events { get; set; }
    IDbSet<EventDetailRow> MissionDetails { get; set; }
    IDbSet<EventGeographyRow> MissionGeography { get; set; }
    IDbSet<MemberRow> Members { get; set; }
    IDbSet<MemberAddressRow> PersonAddress { get; set; }
    IDbSet<MemberContactRow> PersonContact { get; set; }
    IDbSet<MemberUnitDocumentRow> MemberUnitDocuments { get; set; }
    IDbSet<SubjectRow> Subjects { get; set; }
    IDbSet<SubjectGroupRow> SubjectGroups { get; set; }
    IDbSet<SubjectGroupLinkRow> SubjectGroupLinks { get; set; }
    IDbSet<TrainingRecordRow> TrainingRecords { get; set; }
    IDbSet<TrainingCourseRow> TrainingCourses { get; set; }
    IDbSet<DocumentRow> Documents { get; set; }
    IDbSet<TrainingRuleRow> TrainingRules { get; set; }
    IDbSet<UnitRow> Units { get; set; }
    IDbSet<UnitApplicantRow> UnitApplicants { get; set; }
    IDbSet<UnitMembershipRow> UnitMemberships { get; set; }
    IDbSet<UnitStatusRow> UnitStatusTypes { get; set; }
    IDbSet<UnitDocumentRow> UnitDocuments { get; set; }
    IDbSet<ComputedTrainingRecordRow> ComputedTrainingRecords { get; set; }
    IDbSet<SensitiveInfoAccessRow> SensitiveInfoLog { get; set; }

    void RecalculateTrainingAwards();
    void RecalculateTrainingAwards(Guid memberId);
    void RecalculateTrainingAwards(IEnumerable<MemberRow> members);

    AuditLogRow[] GetLog(DateTime since);
    Func<UnitMembershipRow, bool> GetActiveMembershipFilter(Guid? unit, DateTime time);
    IQueryable<MemberRow> GetActiveMembers(Guid? unit, DateTime time, params string[] includes);
    int SaveChanges();

    DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
  }

  public static class IKcsarContextExtensions
  {
    public static IList<TrainingCourseRow> GetCoreCompetencyCourses(this IKcsarContext context)
    {
      var courses = new [] {
                "Clues.WE",
                "CPR",
                "Crime.C", "Crime.WE",
                "FirstAid",
                "Fitness.WE",
                "GPS.PE", "GPS.WE",
                "Helo.C", "Helo.WE",
                "Legal.C", "Legal.WE",
                "Management.WE",
                "Nav.PE", "Nav.WE",
                "Radio.PE", "Radio.WE",
                "Rescue.PE", "Rescue.WE",
                "Safety.PE", "Safety.WE",
                "SearchTech.PE", "SearchTech.WE",
                "Survival.PE", "Survival.WE"
            }.Select(f => "Core/" + f).ToList();
      courses.Add("ICS-100");
      courses.Add("ICS-700");

      return (from c in context.TrainingCourses where courses.Contains(c.DisplayName) select c).AsNoTracking().OrderBy(f => f.DisplayName).ToList();
    }

  }
}
