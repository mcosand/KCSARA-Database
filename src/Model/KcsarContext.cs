
/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Data.Entity;
  using System.Data.Entity.Core;
  using System.Data.Entity.Core.Objects;
  using System.Data.Entity.Infrastructure;
  using System.Linq;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Threading;

  public class KcsarContext : DbContext, IKcsarContext
  {
    public IDbSet<Animal> Animals { get; set; }
    public IDbSet<AnimalMission> AnimalMissions { get; set; }
    public IDbSet<AnimalOwner> AnimalOwners { get; set; }
    public IDbSet<SarEventRow> Events { get; set; }
    public IDbSet<Mission_Old> Missions { get; set; }
    public IDbSet<MissionDetails> MissionDetails { get; set; }
    public IDbSet<EventLogRow> EventLogs { get; set; }
    public IDbSet<MissionRoster_Old> MissionRosters { get; set; }
    public IDbSet<MissionGeography> MissionGeography { get; set; }
    public IDbSet<Member> Members { get; set; }
    public IDbSet<PersonAddress> PersonAddress { get; set; }
    public IDbSet<PersonContact> PersonContact { get; set; }
    public IDbSet<MemberUnitDocument> MemberUnitDocuments { get; set; }
    //       public IDbSet<PersonSubscription> PersonSubscription { get; set; }
    public IDbSet<Subject> Subjects { get; set; }
    public IDbSet<SubjectGroup> SubjectGroups { get; set; }
    public IDbSet<SubjectGroupLink> SubjectGroupLinks { get; set; }
    public IDbSet<Training_Old> Trainings { get; set; }
    public IDbSet<TrainingAward> TrainingAward { get; set; }
    public IDbSet<TrainingCourse> TrainingCourses { get; set; }
    public IDbSet<DocumentRow> Documents { get; set; }
    public IDbSet<TrainingRoster_Old> TrainingRosters { get; set; }
    public IDbSet<TrainingRule> TrainingRules { get; set; }
    public IDbSet<SarUnit> Units { get; set; }
    public IDbSet<UnitApplicant> UnitApplicants { get; set; }
    public IDbSet<UnitMembership> UnitMemberships { get; set; }
    public IDbSet<UnitStatus> UnitStatusTypes { get; set; }
    public IDbSet<UnitDocument> UnitDocuments { get; set; }
    public IDbSet<ComputedTrainingAward> ComputedTrainingAwards { get; set; }
    public IDbSet<TrainingExpirationSummary> TrainingExpirationSummaries { get; set; }
    public IDbSet<CurrentMemberIds> CurrentMemberIds { get; set; }
    public IDbSet<xref_county_id> xref_county_id { get; set; }
    protected IDbSet<AuditLog> AuditLog { get; set; }
    public IDbSet<SensitiveInfoAccess> SensitiveInfoLog { get; set; }
    public IDbSet<Track> Tracks { get; set; }

    public IDbSet<ExternalLogin> ExternalLogins { get; set; }

    public KcsarContext() : this("DataStore") { }

    public KcsarContext(string connName)
      : base(connName)
    {
      this.AuditLog = this.Set<AuditLog>();
    }

    public KcsarContext(string connName, Action<string> logMethod)
      : this(connName)
    {
      this.Database.Log = logMethod;
    }

    public static readonly DateTime MinEntryDate = new DateTime(1945, 1, 1);

    private Dictionary<Type, List<PropertyInfo>> reportingProperties = new Dictionary<Type, List<PropertyInfo>>();
    private Dictionary<string, string> reportingFormats = new Dictionary<string, string>();
    private Random rand = new Random();

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<Mission_Old>().HasOptional(f => f.Details).WithRequired(f => f.Mission).WillCascadeOnDelete();
      modelBuilder.Entity<SarEventRow>().HasMany(f => f.Log).WithRequired(f => f.Event).WillCascadeOnDelete();
      modelBuilder.Entity<Member>().HasOptional(f => f.MedicalInfo).WithRequired(f => f.Member);
      modelBuilder.Entity<Member>().HasMany(f => f.Memberships).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<Member>().HasMany(f => f.Addresses).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<Member>().HasMany(f => f.ContactNumbers).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<Animal>().HasMany(f => f.Owners).WithRequired(f => f.Animal).WillCascadeOnDelete();
      modelBuilder.Entity<Member>().HasMany(f => f.Animals).WithRequired(f => f.Owner).WillCascadeOnDelete();
      modelBuilder.Entity<Member>().HasMany(f => f.ExternalLogins).WithRequired(f => f.Member).WillCascadeOnDelete();
      modelBuilder.Entity<Training_Old>().HasMany(f => f.OfferedCourses).WithMany(f => f.Trainings).Map(cs =>
      {
        cs.MapLeftKey("Training_Id");
        cs.MapRightKey("TrainingCourse_Id");
        cs.ToTable("TrainingTrainingCourses");
      });

      modelBuilder.Entity<SarUnit>().HasMany(f => f.HostedTrainings).WithMany(f => f.HostUnits).Map(cs =>
      {
        cs.MapLeftKey("SarUnit_Id");
        cs.MapRightKey("Training_Id");
        cs.ToTable("TrainingSarUnits");
      });

      modelBuilder.Entity<SarEventRow>()
        .Map<SarEventRow>(m => m.Requires("Discriminator").HasValue(string.Empty).IsRequired())
        .Map<MissionRow>(m => m.Requires("Discriminator").HasValue("Mission").IsRequired())
        .Map<TrainingRow>(m => m.Requires("Discriminator").HasValue("Training").IsRequired());
    }

    public Func<UnitMembership, bool> GetActiveMembershipFilter(Guid? unit, DateTime time)
    {
      // Keep this expression in sync with the one in GetActiveMembers
      return
          g => (!unit.HasValue || g.Unit.Id == unit) && g.Status.IsActive && g.Activated <= time && (g.EndTime == null || g.EndTime > time);
    }

    // Gets members that are active with at least one unit at a specific time, sorted by lastname,firstname
    public IQueryable<Member> GetActiveMembers(Guid? unit, DateTime time, params string[] includes)
    {
      IQueryable<Member> source = this.Members.Include(includes);

      var active = source.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).Where(
          f => f.Memberships.Any(
            // Keep this in sync with the expression in GetActiveMembershipFilter - EntityFramework has problems using the expression
            //  in nested delegates
              g => (!unit.HasValue || g.Unit.Id == unit) && g.Status.IsActive && g.Activated <= time && (g.EndTime == null || g.EndTime > time)
          ));

      return active;
    }


    private ObjectContext comparisonContext = null;
    protected ObjectContext ComparisonContext
    {
      get
      {
        if (this.comparisonContext == null)
        {
          var dbContext = new KcsarContext(this.Database.Connection.ConnectionString, this.Database.Log);
          this.comparisonContext = ((IObjectContextAdapter)dbContext).ObjectContext;
      //    this.comparisonContext.ContextOptions.ProxyCreationEnabled = false;
      //    this.comparisonContext.ContextOptions.LazyLoadingEnabled = true;
        }
        return this.comparisonContext;
      }
    }

    private void AuditChange(ObjectStateEntry entry)
    {
      var obj = entry.Entity as IModelObject;
      if (obj == null) return;

      var audit = new AuditLog
      {
        Action = entry.State.ToString(),
        Changed = DateTime.Now,
        ObjectId = obj.Id,
        User = Thread.CurrentPrincipal.Identity.Name,
        Collection = entry.EntitySet.Name
      };

      switch (entry.State)
      {
        case EntityState.Added:
          audit.Comment = obj.GetReportHtml();
          break;

        case EntityState.Modified:
          string report = string.Format("<b>{0}</b><br/>", entry.Entity);
          foreach (var prop in entry.GetModifiedProperties())
          {
            var displayFormat = entry.Entity.GetType().GetProperty(prop).GetCustomAttribute<DisplayFormatAttribute>();
            string format = displayFormat == null ? "{0}" : displayFormat.DataFormatString;

            report += string.Format(
              "{0}: {1} => {2}<br/>",
              prop,
              string.Format(format, entry.OriginalValues[prop]),
              string.Format(format, entry.CurrentValues[prop]));
          }
          audit.Comment = report;
          break;

        case EntityState.Deleted:
          object original;
          this.ComparisonContext.TryGetObjectByKey(entry.EntityKey, out original);
          audit.Comment = ((IModelObject)original).GetReportHtml();
          break;

        default:
          throw new NotImplementedException("Unhandled state" + entry.State.ToString());
      }
      this.AuditLog.Add(audit);
    }

    public override int SaveChanges()
    {
      ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
      var osm = oc.ObjectStateManager;

      oc.DetectChanges();

      Dictionary<string, IModelObject> updatedRelations = new Dictionary<string, IModelObject>();

      // Deleted objects - we need to fetch more data before we can report what the change was in readable form.
      foreach (ObjectStateEntry entry in osm.GetObjectStateEntries(EntityState.Deleted))
      {
        if (entry.IsRelationship)
        {
          IModelObject obj1 = oc.GetObjectByKey((EntityKey)entry.OriginalValues[0]) as IModelObject;
          IModelObject obj2 = oc.GetObjectByKey((EntityKey)entry.OriginalValues[1]) as IModelObject;
          if (obj1 == null || obj2 == null)
          {
            continue;
          }

          string key = string.Format("{0}{1}", obj1.Id, obj2.Id);
          updatedRelations.Add(key, obj1);
        }
        else
        {
          AuditChange(entry);
        }
      }


      //// Added and modified objects - we can describe the state of the object with
      //// the information already present.
      foreach (ObjectStateEntry entry in
          osm.GetObjectStateEntries(
          EntityState.Added | EntityState.Modified))
      {
        if (entry.IsRelationship)
        {
          var key1 = ((EntityKey)entry.CurrentValues[0]);
          if (key1.IsTemporary) continue;

          var key2 = ((EntityKey)entry.CurrentValues[1]);

          IModelObject obj1 = oc.GetObjectByKey(key1) as IModelObject;
          IModelObject obj2 = oc.GetObjectByKey(key2) as IModelObject;
          if (obj1 == null || obj2 == null)
          {
            continue;
          }

          var audit = new AuditLog
            {
              Action = "Modified",
              Changed = DateTime.Now,
              User = Thread.CurrentPrincipal.Identity.Name,
              ObjectId = obj2.Id,
              Collection = entry.EntitySet.Name,
              Comment = null
            };

          string key = string.Format("{0}{1}", obj1.Id, obj2.Id);
          IModelObject original = null;
          if (updatedRelations.TryGetValue(key, out original))
          {
            audit.Collection = key2.EntitySetName;
            audit.Comment = string.Format("{0}<br/>{1} => {2}", obj2, original, obj1);
            this.AuditLog.Add(audit);
          }
        }
        else if (entry.Entity is IModelObject)
        {
          IModelObject obj = (IModelObject)entry.Entity;

          // Keep track of the change for reporting.
          obj.LastChanged = DateTime.Now;
          obj.ChangedBy = Thread.CurrentPrincipal.Identity.Name;

          AuditChange(entry);
        }
      }

      return base.SaveChanges();
    }

    #region Computed Training
    public void RecalculateTrainingAwards()
    {
      foreach (var member in this.Members)
      {
        RecalculateTrainingAwards(new[] { member }, DateTime.Now);
      }
      // Recalculate the effective training awards for all members.
      //RecalculateTrainingAwards(from a in this.Members select a);
    }

    public void RecalculateTrainingAwards(Guid memberId)
    {
      // Recalculate the effective training awards for a specific member.
      RecalculateTrainingAwards(from m in this.Members where m.Id == memberId select m);
    }

    public void RecalculateTrainingAwards(Guid memberId, DateTime time)
    {
      // Recalculate the effective training awards for a specific member.
      RecalculateTrainingAwards(from m in this.Members where m.Id == memberId select m, time);
    }

    public void RecalculateTrainingAwards(IEnumerable<Member> members)
    {
      RecalculateTrainingAwards(members, DateTime.Now);
    }

    public List<ComputedTrainingAward[]> RecalculateTrainingAwards(IEnumerable<Member> members, DateTime time)
    {
      List<ComputedTrainingAward[]> retVal = new List<ComputedTrainingAward[]>();

      // TODO: only use the rules in effect at time 'time'
      List<TrainingRule> rules = (from r in this.TrainingRules select r).ToList();

      Dictionary<Guid, TrainingCourse> courses = (from c in this.TrainingCourses select c).ToDictionary(x => x.Id);

      foreach (Member m in members)
      {
        foreach (ComputedTrainingAward award in (from a in this.ComputedTrainingAwards where a.Member.Id == m.Id select a))
        {
          this.ComputedTrainingAwards.Remove(award);
        }

        // Sort by expiry and completed dates to handle the case of re-taking a course that doesn't expire.
        var direct = (from a in this.TrainingAward.Include("Course") where a.Member.Id == m.Id && a.Completed <= time select a)
            .OrderBy(f => f.Course.Id).ThenByDescending(f => f.Expiry).ThenByDescending(f => f.Completed);

        Dictionary<Guid, ComputedTrainingAward> awards = new Dictionary<Guid, ComputedTrainingAward>();

        Guid lastCourse = Guid.Empty;
        foreach (TrainingAward a in direct)
        {
          if (this.Entry(a).State == EntityState.Deleted)
          {
            continue;
          }

          if (a.Course.Id != lastCourse)
          {
            var ca = new ComputedTrainingAward(a);
            awards.Add(a.Course.Id, ca);
            this.ComputedTrainingAwards.Add(ca);
            lastCourse = a.Course.Id;
          }
        }

        bool awardInLoop = false;
        do
        {
          awardInLoop = false;

          foreach (TrainingRule rule in rules)
          {
            //  source>result>prerequisite
            string[] fields = rule.RuleText.Split('>');

            if (fields.Length > 2)
            {
              var prereqs = fields[2].Split('+');
              // Keep going only if /all/ of the prereqs are met by /any/ of the existing awards, 
              if (!prereqs.All(f => awards.Keys.Any(g => g.ToString().Equals(f, StringComparison.OrdinalIgnoreCase))))
              {
                continue;
              }
            }

            if (fields[0].StartsWith("Mission"))
            {
              //Mission(12:%:36)
              Match match = Regex.Match(fields[0], @"Mission\((\d+):([^:]+):(\d+)\)", RegexOptions.IgnoreCase);
              if (match.Success == false)
              {
                throw new InvalidOperationException("Can't understand rule: " + fields[0]);
              }

              int requiredHours = int.Parse(match.Groups[1].Value);
              string missionType = match.Groups[2].Value;
              int monthSpan = int.Parse(match.Groups[3].Value);

              var missions = (from r in this.MissionRosters where r.Person.Id == m.Id && r.TimeIn < time select r);
              if (missionType != "%")
              {
                missions = missions.Where(x => x.Mission.MissionType.Contains(missionType));
              }
              missions = missions.OrderByDescending(x => x.TimeIn);

              double sum = 0;
              DateTime startDate = DateTime.Now;
              foreach (MissionRoster_Old roster in missions)
              {
                if (roster.TimeIn.HasValue && (roster.InternalRole != MissionRoster_Old.ROLE_IN_TOWN && roster.InternalRole != MissionRoster_Old.ROLE_NO_ROLE))
                {
                  startDate = roster.TimeIn.Value;
                  sum += roster.Hours ?? 0.0;

                  if (sum > requiredHours)
                  {
                    awardInLoop |= RewardTraining(m, courses, awards, rule, startDate, startDate.AddMonths(monthSpan), fields[1]);
                    break;
                  }
                }
              }
            }
            else
            {
              //Guid? sourceCourse = fields[0].ToGuid();

              //if (sourceCourse == null)
              //{
              //    throw new InvalidOperationException("Unknown rule type: " + rule.Id);
              //}

              //if (awards.ContainsKey(sourceCourse.Value))
              //{
              //    System.Diagnostics.Debug.WriteLineIf(m.LastName == "Kedan", string.Format("Applying rule using {0}, {1}", courses[sourceCourse.Value].DisplayName, awards[sourceCourse.Value].Completed));
              //    RewardTraining(m, courses, awards, rule, awards[sourceCourse.Value].Completed, awards[sourceCourse.Value].Expiry, fields[1]);
              //}
              Guid?[] sources = fields[0].Split('+').Select(f => f.ToGuid()).ToArray();

              if (sources.Any(f => f == null))
              {
                throw new InvalidOperationException("Unknown rule type: " + rule.Id);
              }

              if (sources.All(f => awards.ContainsKey(f.Value)))
              {
                DateTime? completed = sources.Max(f => awards[f.Value].Completed);
                DateTime? expiry = null;
                if (sources.Any(f => awards[f.Value].Expiry != null))
                {
                  expiry = sources.Min(f => awards[f.Value].Expiry ?? DateTime.MaxValue);
                }
                awardInLoop |= RewardTraining(m, courses, awards, rule, completed, expiry, fields[1]);
              }
            }
          }
        } while (awardInLoop);
        retVal.Add(awards.Values.ToArray());
      }
      return retVal;
    }

    private bool RewardTraining(Member m, Dictionary<Guid, TrainingCourse> courses, Dictionary<Guid, ComputedTrainingAward> awards, TrainingRule rule, DateTime? completed, DateTime? expiry, string newAwardsString)
    {
      IEnumerable<string> results = newAwardsString.Split('+');
      bool awarded = false;

      if (completed < (rule.OfferedFrom ?? DateTime.MinValue) || completed > (rule.OfferedUntil ?? DateTime.MaxValue))
      {
        return false;
      }

      foreach (string result in results)
      {
        string[] parts = result.Split(':');
        Guid course = new Guid(parts[0]);

        if (!courses.ContainsKey(course))
        {
          throw new InvalidOperationException("Found bad rule: Adds course with ID" + course.ToString());
        }

        if (parts.Length > 1)
        {
          if (parts[1] == "default")
          {
            if (courses[course].ValidMonths.HasValue)
            {
              expiry = completed.Value.AddMonths(courses[course].ValidMonths.Value);
            }
            else
            {
              expiry = null;
            }
          }
          else
          {
            expiry = completed.Value.AddMonths(int.Parse(parts[1]));
          }
        }


        if (awards.ContainsKey(course) && expiry > awards[course].Expiry)
        {
          awards[course].Completed = completed;
          awards[course].Expiry = expiry;
          awards[course].Rule = rule;
          awarded = true;
          System.Diagnostics.Debug.WriteLineIf(m.LastName == "Kedan", string.Format("Updating existing record {0}, new expiry: {1}", courses[course].DisplayName, expiry));
        }
        else if (!awards.ContainsKey(course))
        {
          ComputedTrainingAward newAward = new ComputedTrainingAward { Course = courses[course], Member = m, Completed = completed, Expiry = expiry, Rule = rule };
          awards.Add(course, newAward);
          this.ComputedTrainingAwards.Add(newAward);
          awarded = true;
          System.Diagnostics.Debug.WriteLineIf(m.LastName == "Kedan", string.Format("Add new record {0}, new expiry: {1}", courses[course].DisplayName, expiry));
        }
      }
      return awarded;
    }
    #endregion

    public AuditLog[] GetLog(DateTime since)
    {
      var log = this.AuditLog.AsNoTracking().Where(f => f.Changed >= since).OrderByDescending(f => f.Changed).AsEnumerable();
      return log.Select(f => f.GetCopy()).ToArray();
    }
  }
}
