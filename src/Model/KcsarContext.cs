
/*
 * Copyright 2009-2016 Matthew Cosand
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
  using System.Data.SqlClient;
  using System.Linq;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Threading;
  using System.Threading.Tasks;
  using Events;
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
    public IDbSet<MemberRow> Members { get; set; }
    public IDbSet<PersonAddress> PersonAddress { get; set; }
    public IDbSet<PersonContact> PersonContact { get; set; }
    public IDbSet<MemberUnitDocument> MemberUnitDocuments { get; set; }
    //       public IDbSet<PersonSubscription> PersonSubscription { get; set; }
    public IDbSet<Subject> Subjects { get; set; }
    public IDbSet<SubjectGroup> SubjectGroups { get; set; }
    public IDbSet<SubjectGroupLink> SubjectGroupLinks { get; set; }
    public IDbSet<Training_Old> Trainings { get; set; }
    public IDbSet<TrainingAwardRow> TrainingAward { get; set; }
    public IDbSet<TrainingCourse> TrainingCourses { get; set; }
    public IDbSet<DocumentRow> Documents { get; set; }
    public IDbSet<TrainingRoster_Old> TrainingRosters { get; set; }
    public IDbSet<TrainingRule> TrainingRules { get; set; }
    public IDbSet<SarUnitRow> Units { get; set; }
    public IDbSet<UnitApplicant> UnitApplicants { get; set; }
    public IDbSet<UnitMembership> UnitMemberships { get; set; }
    public IDbSet<UnitStatus> UnitStatusTypes { get; set; }
    public IDbSet<UnitDocument> UnitDocuments { get; set; }
    public IDbSet<ComputedTrainingAwardRow> ComputedTrainingAwards { get; set; }
    public IDbSet<TrainingExpirationSummary> TrainingExpirationSummaries { get; set; }
    public IDbSet<CurrentMemberIds> CurrentMemberIds { get; set; }
    public IDbSet<xref_county_id> xref_county_id { get; set; }
    protected IDbSet<AuditLog> AuditLog { get; set; }
    public IDbSet<SensitiveInfoAccess> SensitiveInfoLog { get; set; }
    public IDbSet<Track> Tracks { get; set; }

    public IDbSet<ExternalLogin> ExternalLogins { get; set; }

    public Task<List<T>> EventDashboardStatistics<T>(string eventType)
    {
      return Database.SqlQuery<T>("EventDashboardStatistics @discriminator", new SqlParameter("discriminator", eventType)).ToListAsync();
    }

    public bool SystemUpdates { get; set; }

    public KcsarContext() : this("DataStore", null) { }

    public KcsarContext(string connName, Func<string> usernameGetter)
      : base(connName)
    {
      this.AuditLog = this.Set<AuditLog>();
      this.usernameGetter = usernameGetter ?? (() => Thread.CurrentPrincipal.Identity.Name);
      this.SystemUpdates = false;
    }

    public KcsarContext(string connName, Action<string> logMethod)
      : this(connName, null)
    {
      this.Database.Log = logMethod;
    }

    public static readonly DateTime MinEntryDate = new DateTime(1945, 1, 1);

    private readonly Func<string> usernameGetter;

    private Dictionary<Type, List<PropertyInfo>> reportingProperties = new Dictionary<Type, List<PropertyInfo>>();
    private Dictionary<string, string> reportingFormats = new Dictionary<string, string>();
    private Random rand = new Random();

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<Mission_Old>().HasOptional(f => f.Details).WithRequired(f => f.Mission).WillCascadeOnDelete();
      modelBuilder.Entity<SarEventRow>().HasMany(f => f.Log).WithRequired(f => f.Event).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasOptional(f => f.MedicalInfo).WithRequired(f => f.Member);
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Memberships).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Addresses).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.ContactNumbers).WithRequired(f => f.Person).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.EmergencyContacts).WithRequired(f => f.Member).WillCascadeOnDelete();

      modelBuilder.Entity<Animal>().HasMany(f => f.Owners).WithRequired(f => f.Animal).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Animals).WithRequired(f => f.Owner).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.ExternalLogins).WithRequired(f => f.Member).WillCascadeOnDelete();
      modelBuilder.Entity<Training_Old>().HasMany(f => f.OfferedCourses).WithMany(f => f.Trainings).Map(cs =>
      {
        cs.MapLeftKey("Training_Id");
        cs.MapRightKey("TrainingCourse_Id");
        cs.ToTable("TrainingTrainingCourses");
      });

      modelBuilder.Entity<SarUnitRow>().HasMany(f => f.HostedTrainings).WithMany(f => f.HostUnits).Map(cs =>
      {
        cs.MapLeftKey("SarUnit_Id");
        cs.MapRightKey("Training_Id");
        cs.ToTable("TrainingSarUnits");
      });

      modelBuilder.Entity<ComputedTrainingAwardRow>().HasOptional(f => f.RosterEntry).WithMany();

      modelBuilder.Entity<SarEventRow>()
        .Map<SarEventRow>(m => m.Requires("Discriminator").HasValue(string.Empty).IsRequired())
        .Map<MissionRow>(m => m.Requires("Discriminator").HasValue("Mission").IsRequired())
        .Map<TrainingRow>(m => m.Requires("Discriminator").HasValue("Training").IsRequired());
      modelBuilder.Entity<EventLogRow>().HasKey(f => new { f.EventId, f.Id });
    }

    public Func<UnitMembership, bool> GetActiveMembershipFilter(Guid? unit, DateTime time)
    {
      // Keep this expression in sync with the one in GetActiveMembers
      return
          g => (!unit.HasValue || g.Unit.Id == unit) && g.Status.IsActive && g.Activated <= time && (g.EndTime == null || g.EndTime > time);
    }

    // Gets members that are active with at least one unit at a specific time, sorted by lastname,firstname
    public IQueryable<MemberRow> GetActiveMembers(Guid? unit, DateTime time, params string[] includes)
    {
      IQueryable<MemberRow> source = this.Members.Include(includes);

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
      if (!SystemUpdates) AuditLog.Add(audit);
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
            if (!SystemUpdates) this.AuditLog.Add(audit);
          }
        }
        else if (entry.Entity is IModelObject)
        {
          IModelObject obj = (IModelObject)entry.Entity;

          if (!SystemUpdates)
          {
            // Keep track of the change for reporting.
            obj.LastChanged = DateTime.Now;
            obj.ChangedBy = usernameGetter();

            AuditChange(entry);
          }
        }
      }

      return base.SaveChanges();
    }

    public AuditLog[] GetLog(DateTime since)
    {
      var log = this.AuditLog.AsNoTracking().Where(f => f.Changed >= since).OrderByDescending(f => f.Changed).AsEnumerable();
      return log.Select(f => f.GetCopy()).ToArray();
    }
  }
}
