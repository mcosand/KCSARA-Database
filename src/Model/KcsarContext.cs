/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Data.Entity;
  using System.Data.Entity.Core;
  using System.Data.Entity.Core.Objects;
  using System.Data.Entity.Infrastructure;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using Kcsar.Database.Data.Events;

  public class KcsarContext : DbContext, IKcsarContext
  {
    public IDbSet<AnimalRow> Animals { get; set; }
    public IDbSet<AnimalEventRow> AnimalMissions { get; set; }
    public IDbSet<AnimalOwnerRow> AnimalOwners { get; set; }
    public IDbSet<SarEventRow> Events { get; set; }
    public IDbSet<EventDetailRow> MissionDetails { get; set; }
    public IDbSet<EventGeographyRow> MissionGeography { get; set; }
    public IDbSet<MemberRow> Members { get; set; }
    public IDbSet<MemberAddressRow> PersonAddress { get; set; }
    public IDbSet<MemberContactRow> PersonContact { get; set; }
    public IDbSet<MemberUnitDocumentRow> MemberUnitDocuments { get; set; }
    public IDbSet<SubjectRow> Subjects { get; set; }
    public IDbSet<SubjectGroupRow> SubjectGroups { get; set; }
    public IDbSet<SubjectGroupLinkRow> SubjectGroupLinks { get; set; }
    public IDbSet<TrainingRecordRow> TrainingRecords { get; set; }
    public IDbSet<TrainingCourseRow> TrainingCourses { get; set; }
    public IDbSet<DocumentRow> Documents { get; set; }
    public IDbSet<TrainingRuleRow> TrainingRules { get; set; }
    public IDbSet<UnitRow> Units { get; set; }
    public IDbSet<UnitApplicantRow> UnitApplicants { get; set; }
    public IDbSet<UnitMembershipRow> UnitMemberships { get; set; }
    public IDbSet<UnitStatusRow> UnitStatusTypes { get; set; }
    public IDbSet<UnitDocumentRow> UnitDocuments { get; set; }
    public IDbSet<ComputedTrainingRecordRow> ComputedTrainingAwards { get; set; }
    protected IDbSet<AuditLogRow> AuditLog { get; set; }
    public IDbSet<SensitiveInfoAccessRow> SensitiveInfoLog { get; set; }

    public KcsarContext() : this("DataStore") { }

    public KcsarContext(string connName)
      : base(connName)
    {
      this.AuditLog = this.Set<AuditLogRow>();
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
      modelBuilder.Entity<SarEventRow>().HasMany(f => f.Roster).WithRequired(f => f.Event).WillCascadeOnDelete(false);
      modelBuilder.Entity<ParticipantRow>().HasOptional(f => f.Member).WithMany(f => f.Participation).WillCascadeOnDelete(false);
      modelBuilder.Entity<ParticipatingUnitRow>().HasOptional(f => f.MemberUnit).WithMany(f => f.Participation).WillCascadeOnDelete(false);
      modelBuilder.Entity<EventRosterRow>().HasRequired(f => f.Participant).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<EventRosterRow>().HasOptional(f => f.Unit).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<ParticipantStatusRow>().HasRequired(f => f.Participant).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<ParticipantStatusRow>().HasOptional(f => f.Unit).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<EventLogRow>().HasOptional(f => f.Participant).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<ParticipantEventTimeline>().HasOptional(f => f.Participant).WithMany().WillCascadeOnDelete(false);
      modelBuilder.Entity<EventDetailRow>().HasOptional(f => f.Member).WithMany(f => f.MissionDetails).WillCascadeOnDelete(false);
      modelBuilder.Entity<SarEventRow>().HasOptional(f => f.Details).WithRequired(f => f.Event).WillCascadeOnDelete();
      modelBuilder.Entity<TrainingRow>().HasMany(f => f.OfferedCourses).WithMany(f => f.Trainings);
      modelBuilder.Entity<MemberRow>().HasOptional(f => f.MedicalInfo).WithRequired(f => f.Member);
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Memberships).WithRequired(f => f.Member).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Addresses).WithRequired(f => f.Member).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.ContactNumbers).WithRequired(f => f.Member).WillCascadeOnDelete();
      modelBuilder.Entity<AnimalRow>().HasMany(f => f.Owners).WithRequired(f => f.Animal).WillCascadeOnDelete();
      modelBuilder.Entity<MemberRow>().HasMany(f => f.Animals).WithRequired(f => f.Owner).WillCascadeOnDelete();
      modelBuilder.Entity<UnitStatusRow>().HasMany(f => f.Memberships).WithRequired(f => f.Status).WillCascadeOnDelete(false);
    }

    public Func<UnitMembershipRow, bool> GetActiveMembershipFilter(Guid? unit, DateTime time)
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
      var obj = entry.Entity as IModelObjectRow;
      if (obj == null) return;

      var audit = new AuditLogRow
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
          audit.Comment = ((IModelObjectRow)original).GetReportHtml();
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

      Dictionary<string, IModelObjectRow> updatedRelations = new Dictionary<string, IModelObjectRow>();

      // Deleted objects - we need to fetch more data before we can report what the change was in readable form.
      foreach (ObjectStateEntry entry in osm.GetObjectStateEntries(EntityState.Deleted))
      {
        if (entry.IsRelationship)
        {
          IModelObjectRow obj1 = oc.GetObjectByKey((EntityKey)entry.OriginalValues[0]) as IModelObjectRow;
          IModelObjectRow obj2 = oc.GetObjectByKey((EntityKey)entry.OriginalValues[1]) as IModelObjectRow;
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

          IModelObjectRow obj1 = oc.GetObjectByKey(key1) as IModelObjectRow;
          IModelObjectRow obj2 = oc.GetObjectByKey(key2) as IModelObjectRow;
          if (obj1 == null || obj2 == null)
          {
            continue;
          }

          var audit = new AuditLogRow
            {
              Action = "Modified",
              Changed = DateTime.Now,
              User = Thread.CurrentPrincipal.Identity.Name,
              ObjectId = obj2.Id,
              Collection = entry.EntitySet.Name,
              Comment = null
            };

          string key = string.Format("{0}{1}", obj1.Id, obj2.Id);
          IModelObjectRow original = null;
          if (updatedRelations.TryGetValue(key, out original))
          {
            audit.Collection = key2.EntitySetName;
            audit.Comment = string.Format("{0}<br/>{1} => {2}", obj2, original, obj1);
            this.AuditLog.Add(audit);
          }
        }
        else if (entry.Entity is IModelObjectRow)
        {
          IModelObjectRow obj = (IModelObjectRow)entry.Entity;

          // Keep track of the change for reporting.
          obj.LastChanged = DateTime.Now;
          obj.ChangedBy = Thread.CurrentPrincipal.Identity.Name;

          AuditChange(entry);
          DocumentRow doc = obj as DocumentRow;
          if (doc != null)
          {
            SaveDocumentFile(doc);
          }
        }
      }

      return base.SaveChanges();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="d"></param>
    private void SaveDocumentFile(DocumentRow d)
    {
      if (string.IsNullOrWhiteSpace(d.StorePath))
      {
        string path = string.Empty;
        for (int i = 0; i < DocumentRow.StorageTreeDepth; i++)
        {
          path += ((i > 0) ? "\\" : "") + rand.Next(DocumentRow.StorageTreeSpan).ToString();
        }
        if (!System.IO.Directory.Exists(DocumentRow.StorageRoot + path))
        {
          System.IO.Directory.CreateDirectory(DocumentRow.StorageRoot + path);
        }
        path += "\\" + d.Id.ToString();
        d.StorePath = path;
      }
      System.IO.File.WriteAllBytes(DocumentRow.StorageRoot + d.StorePath, d.Contents);
    }

    public void RemoveStaleDocumentFiles()
    {
      HashSet<string> dbFiles = new HashSet<string>(
        this.Documents
        .Select(f => f.StorePath).Distinct()
        .AsNoTracking().AsEnumerable());

      int rootLength = DocumentRow.StorageRoot.Length;
      foreach (var file in Directory.GetFiles(DocumentRow.StorageRoot, "*.*", SearchOption.AllDirectories))
      {
        if (dbFiles.Contains(file.Substring(rootLength)))
          continue;

        File.Delete(file);
        string path = file;
        for (int i = 0; i < DocumentRow.StorageTreeDepth; i++)
        {
          path = Path.GetDirectoryName(path);
          if (Directory.GetDirectories(path).Length + Directory.GetFiles(path).Length == 0)
          {
            Directory.Delete(path);
          }
        }
      }
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

    public void RecalculateTrainingAwards(IEnumerable<MemberRow> members)
    {
      RecalculateTrainingAwards(members, DateTime.Now);
    }

    public List<ComputedTrainingRecordRow[]> RecalculateTrainingAwards(IEnumerable<MemberRow> members, DateTime time)
    {
      List<ComputedTrainingRecordRow[]> retVal = new List<ComputedTrainingRecordRow[]>();

      // TODO: only use the rules in effect at time 'time'
      List<TrainingRuleRow> rules = (from r in this.TrainingRules select r).ToList();

      Dictionary<Guid, TrainingCourseRow> courses = (from c in this.TrainingCourses select c).ToDictionary(x => x.Id);

      foreach (MemberRow m in members)
      {
        foreach (ComputedTrainingRecordRow award in (from a in this.ComputedTrainingAwards where a.Member.Id == m.Id select a))
        {
          this.ComputedTrainingAwards.Remove(award);
        }

        // Sort by expiry and completed dates to handle the case of re-taking a course that doesn't expire.
        var direct = (from a in this.TrainingRecords.Include("Course") where a.Member.Id == m.Id && a.Completed <= time select a)
            .OrderBy(f => f.Course.Id).ThenByDescending(f => f.Expiry).ThenByDescending(f => f.Completed);

        Dictionary<Guid, ComputedTrainingRecordRow> awards = new Dictionary<Guid, ComputedTrainingRecordRow>();

        Guid lastCourse = Guid.Empty;
        foreach (TrainingRecordRow a in direct)
        {
          if (this.Entry(a).State == EntityState.Deleted)
          {
            continue;
          }

          if (a.Course.Id != lastCourse)
          {
            var ca = new ComputedTrainingRecordRow(a);
            awards.Add(a.Course.Id, ca);
            this.ComputedTrainingAwards.Add(ca);
            lastCourse = a.Course.Id;
          }
        }

        bool awardInLoop = false;
        do
        {
          awardInLoop = false;

          foreach (TrainingRuleRow rule in rules)
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
              throw new NotImplementedException("reimplement");
              ////Mission(12:%:36)
              //Match match = Regex.Match(fields[0], @"Mission\((\d+):([^:]+):(\d+)\)", RegexOptions.IgnoreCase);
              //if (match.Success == false)
              //{
              //  throw new InvalidOperationException("Can't understand rule: " + fields[0]);
              //}

              //int requiredHours = int.Parse(match.Groups[1].Value);
              //string missionType = match.Groups[2].Value;
              //int monthSpan = int.Parse(match.Groups[3].Value);

              //var missions = (from r in this.MissionRosters where r.Person.Id == m.Id && r.TimeIn < time select r);
              //if (missionType != "%")
              //{
              //  missions = missions.Where(x => x.Mission.MissionType.Contains(missionType));
              //}
              //missions = missions.OrderByDescending(x => x.TimeIn);

              //double sum = 0;
              //DateTime startDate = DateTime.Now;
              //foreach (MissionRoster roster in missions)
              //{
              //  if (roster.TimeIn.HasValue && (roster.InternalRole != MissionRoster.ROLE_IN_TOWN && roster.InternalRole != MissionRoster.ROLE_NO_ROLE))
              //  {
              //    startDate = roster.TimeIn.Value;
              //    sum += roster.Hours ?? 0.0;

              //    if (sum > requiredHours)
              //    {
              //      awardInLoop |= RewardTraining(m, courses, awards, rule, startDate, startDate.AddMonths(monthSpan), fields[1]);
              //      break;
              //    }
              //  }
              //}
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

    private bool RewardTraining(MemberRow m, Dictionary<Guid, TrainingCourseRow> courses, Dictionary<Guid, ComputedTrainingRecordRow> awards, TrainingRuleRow rule, DateTime? completed, DateTime? expiry, string newAwardsString)
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
          ComputedTrainingRecordRow newAward = new ComputedTrainingRecordRow { Course = courses[course], Member = m, Completed = completed, Expiry = expiry, Rule = rule };
          awards.Add(course, newAward);
          this.ComputedTrainingAwards.Add(newAward);
          awarded = true;
          System.Diagnostics.Debug.WriteLineIf(m.LastName == "Kedan", string.Format("Add new record {0}, new expiry: {1}", courses[course].DisplayName, expiry));
        }
      }
      return awarded;
    }
    #endregion

    public AuditLogRow[] GetLog(DateTime since)
    {
      var log = this.AuditLog.AsNoTracking().Where(f => f.Changed >= since).OrderByDescending(f => f.Changed).AsEnumerable();
      return log.Select(f => f.GetCopy()).ToArray();
    }
  }
}
