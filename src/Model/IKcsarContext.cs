/*
 * Copyright 2013-2016 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Infrastructure;
  using System.Linq;
  using Events;
  public interface IKcsarContext : IDisposable
  {
    IDbSet<Animal> Animals { get; set; }
    IDbSet<AnimalMission> AnimalMissions { get; set; }
    IDbSet<AnimalOwner> AnimalOwners { get; set; }
    IDbSet<SarEventRow> Events { get; set; }
    IDbSet<Mission_Old> Missions { get; set; }
    IDbSet<MissionDetails> MissionDetails { get; set; }
    IDbSet<EventLogRow> EventLogs { get; set; }
    IDbSet<MissionRoster_Old> MissionRosters { get; set; }
    IDbSet<MissionGeography> MissionGeography { get; set; }
    IDbSet<MemberRow> Members { get; set; }
    IDbSet<PersonAddress> PersonAddress { get; set; }
    IDbSet<PersonContact> PersonContact { get; set; }
    IDbSet<MemberUnitDocument> MemberUnitDocuments { get; set; }
    IDbSet<Subject> Subjects { get; set; }
    IDbSet<SubjectGroup> SubjectGroups { get; set; }
    IDbSet<SubjectGroupLink> SubjectGroupLinks { get; set; }
    IDbSet<Training_Old> Trainings { get; set; }
    IDbSet<TrainingAwardRow> TrainingAward { get; set; }
    IDbSet<TrainingCourse> TrainingCourses { get; set; }
    IDbSet<DocumentRow> Documents { get; set; }
    IDbSet<TrainingRoster_Old> TrainingRosters { get; set; }
    IDbSet<TrainingRule> TrainingRules { get; set; }
    IDbSet<SarUnitRow> Units { get; set; }
    IDbSet<UnitApplicant> UnitApplicants { get; set; }
    IDbSet<UnitMembership> UnitMemberships { get; set; }
    IDbSet<UnitStatus> UnitStatusTypes { get; set; }
    IDbSet<UnitDocument> UnitDocuments { get; set; }
    IDbSet<ComputedTrainingAwardRow> ComputedTrainingAwards { get; set; }
    IDbSet<TrainingExpirationSummary> TrainingExpirationSummaries { get; set; }
    IDbSet<CurrentMemberIds> CurrentMemberIds { get; set; }
    IDbSet<xref_county_id> xref_county_id { get; set; }
    IDbSet<SensitiveInfoAccess> SensitiveInfoLog { get; set; }

    IDbSet<Track> Tracks { get; set; }

    IEnumerable<T> EventDashboardStatistics<T>(string eventType);

    void RecalculateTrainingAwards();
    void RecalculateTrainingAwards(Guid memberId);
    void RecalculateTrainingAwards(IEnumerable<MemberRow> members);

    AuditLog[] GetLog(DateTime since);
    Func<UnitMembership, bool> GetActiveMembershipFilter(Guid? unit, DateTime time);
    IQueryable<MemberRow> GetActiveMembers(Guid? unit, DateTime time, params string[] includes);
    int SaveChanges();

    DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
  }

  public static class IKcsarContextExtensions
  {
    public static IList<TrainingCourse> GetCoreCompetencyCourses(this IKcsarContext context)
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
