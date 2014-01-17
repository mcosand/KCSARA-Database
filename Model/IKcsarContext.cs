/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Infrastructure;
  using System.Linq;

  public interface IKcsarContext
  {
    IDbSet<Animal> Animals { get; set; }
    IDbSet<AnimalMission> AnimalMissions { get; set; }
    IDbSet<AnimalOwner> AnimalOwners { get; set; }
    IDbSet<Mission> Missions { get; set; }
    IDbSet<MissionDetails> MissionDetails { get; set; }
    IDbSet<MissionLog> MissionLog { get; set; }
    IDbSet<MissionRoster> MissionRosters { get; set; }
    IDbSet<MissionGeography> MissionGeography { get; set; }
    IDbSet<Member> Members { get; set; }
    IDbSet<PersonAddress> PersonAddress { get; set; }
    IDbSet<PersonContact> PersonContact { get; set; }
    IDbSet<MemberUnitDocument> MemberUnitDocuments { get; set; }
    IDbSet<Subject> Subjects { get; set; }
    IDbSet<SubjectGroup> SubjectGroups { get; set; }
    IDbSet<SubjectGroupLink> SubjectGroupLinks { get; set; }
    IDbSet<Training> Trainings { get; set; }
    IDbSet<TrainingAward> TrainingAward { get; set; }
    IDbSet<TrainingCourse> TrainingCourses { get; set; }
    IDbSet<Document> Documents { get; set; }
    IDbSet<TrainingRoster> TrainingRosters { get; set; }
    IDbSet<TrainingRule> TrainingRules { get; set; }
    IDbSet<SarUnit> Units { get; set; }
    IDbSet<UnitApplicant> UnitApplicants { get; set; }
    IDbSet<UnitMembership> UnitMemberships { get; set; }
    IDbSet<UnitStatus> UnitStatusTypes { get; set; }
    IDbSet<UnitDocument> UnitDocuments { get; set; }
    IDbSet<ComputedTrainingAward> ComputedTrainingAwards { get; set; }
    IDbSet<TrainingExpirationSummary> TrainingExpirationSummaries { get; set; }
    IDbSet<CurrentMemberIds> CurrentMemberIds { get; set; }
    IDbSet<xref_county_id> xref_county_id { get; set; }
    IDbSet<SensitiveInfoAccess> SensitiveInfoLog { get; set; }

    void RecalculateTrainingAwards();
    void RecalculateTrainingAwards(Guid memberId);
    void RecalculateTrainingAwards(IEnumerable<Member> members);

    AuditLog[] GetLog(DateTime since);
    Func<UnitMembership, bool> GetActiveMembershipFilter(Guid? unit, DateTime time);
    IQueryable<Member> GetActiveMembers(Guid? unit, DateTime time, params string[] includes);
    int SaveChanges();

    DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
  }

  public static class IKcsarContextExtensions
  {
    public static IList<TrainingCourse> GetCoreCompetencyCourses(this IKcsarContext context)
    {
      var courses = new[] {
                "Clues",
                "Crime",
                "FA",
                "Fitness",
                "GPS.P", "GPS.W",
                "Helicopter",
                "Legal",
                "Management",
                "Nav.P", "Nav.W",
                "Radio",
                "Rescue.P", "Rescue.W",
                "Safety.P", "Safety.W",
                "Search.P", "Search.W",
                "Survival.P", "Survival.W"
            }.Select(f => "Core/" + f).ToArray();

      return (from c in context.TrainingCourses where courses.Contains(c.DisplayName) select c).AsNoTracking().OrderBy(f => f.DisplayName).ToList();
    }

  }
}
