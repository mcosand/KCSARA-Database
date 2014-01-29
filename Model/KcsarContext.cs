/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Data.Entity.Core;
  using System.Data.Entity.Core.Objects;
  using System.Data.Entity.Core.Objects.DataClasses;
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
    public IDbSet<Mission> Missions { get; set; }
    public IDbSet<MissionDetails> MissionDetails { get; set; }
    public IDbSet<MissionLog> MissionLog { get; set; }
    public IDbSet<MissionRoster> MissionRosters { get; set; }
    public IDbSet<MissionGeography> MissionGeography { get; set; }
    public IDbSet<Member> Members { get; set; }
    public IDbSet<PersonAddress> PersonAddress { get; set; }
    public IDbSet<PersonContact> PersonContact { get; set; }
    public IDbSet<MemberUnitDocument> MemberUnitDocuments { get; set; }
    public IDbSet<Subject> Subjects { get; set; }
    public IDbSet<SubjectGroup> SubjectGroups { get; set; }
    public IDbSet<SubjectGroupLink> SubjectGroupLinks { get; set; }
    public IDbSet<Training> Trainings { get; set; }
    public IDbSet<TrainingAward> TrainingAward { get; set; }
    public IDbSet<TrainingCourse> TrainingCourses { get; set; }
    public IDbSet<Document> Documents { get; set; }
    public IDbSet<TrainingRoster> TrainingRosters { get; set; }
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

    public KcsarContext(string connectionString) : base(connectionString)
    {
      this.AuditLog = this.Set<AuditLog>();
    }

    public KcsarContext()
      : this("DataStore")
    {
    }

    public static readonly DateTime MinEntryDate = new DateTime(1945, 1, 1);

    private Dictionary<Type, List<PropertyInfo>> reportingProperties = new Dictionary<Type, List<PropertyInfo>>();
    private Dictionary<string, string> reportingFormats = new Dictionary<string, string>();

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<Mission>().HasOptional(f => f.Details).WithRequired(f => f.Mission);
      modelBuilder.Entity<Mission>().HasOptional(f => f.ResponseStatus).WithRequired(f => f.Mission)
        .WillCascadeOnDelete(true);
      modelBuilder.Entity<Member>().HasOptional(f => f.MedicalInfo).WithRequired(f => f.Member);
      modelBuilder.Entity<MissionRespondingUnit>().HasMany(f => f.Responders).WithRequired(f => f.RespondingUnit)
        .HasForeignKey(f => f.RespondingUnitId).WillCascadeOnDelete(true);
      modelBuilder.Entity<Mission>().HasMany(f => f.Responders).WithRequired(f => f.Mission)
        .HasForeignKey(f => f.MissionId).WillCascadeOnDelete(false);
      modelBuilder.Entity<SarUnit>().HasMany(f => f.MissionResponses).WithRequired(f => f.Unit)
        .HasForeignKey(f => f.UnitId).WillCascadeOnDelete(true);
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

    public override int SaveChanges()
    {
      List<RuleViolation> errors = new List<RuleViolation>();

      KcsarContext comparisonContext = new KcsarContext(this.Database.Connection.ConnectionString);

      List<AuditLog> changes = new List<AuditLog>();

      // Validate the state of each entity in the context
      // before SaveChanges can succeed.
      Random rand = new Random();

      ObjectContext oc = ((IObjectContextAdapter)this).ObjectContext;
      var osm = oc.ObjectStateManager;

      oc.DetectChanges();

      // Added and modified objects - we can describe the state of the object with
      // the information already present.
      foreach (ObjectStateEntry entry in
          osm.GetObjectStateEntries(
          EntityState.Added | EntityState.Modified))
      {
        // Do Validation
        if (entry.Entity is IValidatedEntity)
        {
          IValidatedEntity validate = (IValidatedEntity)entry.Entity;
          if (!validate.Validate())
          {
            errors.AddRange(validate.Errors);
          }
          else
          {
            Document d = entry.Entity as Document;
            if (d != null)
            {
              if (string.IsNullOrWhiteSpace(d.StorePath))
              {
                string path = string.Empty;
                for (int i = 0; i < Document.StorageTreeDepth; i++)
                {
                  path += ((i > 0) ? "\\" : "") + rand.Next(Document.StorageTreeSpan).ToString();
                }
                if (!System.IO.Directory.Exists(Document.StorageRoot + path))
                {
                  System.IO.Directory.CreateDirectory(Document.StorageRoot + path);
                }
                path += "\\" + d.Id.ToString();
                d.StorePath = path;
              }
              System.IO.File.WriteAllBytes(Document.StorageRoot + d.StorePath, d.Contents);
            }
            // New values are valid

            if (entry.Entity is IModelObject)
            {
              IModelObject obj = (IModelObject)entry.Entity;

              // Keep track of the change for reporting.
              obj.LastChanged = DateTime.Now;
              obj.ChangedBy = Thread.CurrentPrincipal.Identity.Name;

              IModelObject original = (entry.State == EntityState.Added) ? null : GetOriginalVersion(comparisonContext, entry);


              if (original == null)
              {
                changes.Add(new AuditLog
                {
                  Action = entry.State.ToString(),
                  Comment = obj.GetReportHtml(),
                  Collection = entry.EntitySet.Name,
                  Changed = DateTime.Now,
                  ObjectId = obj.Id,
                  User = Thread.CurrentPrincipal.Identity.Name
                });
              }
              else
              {
                string report = string.Format("<b>{0}</b><br/>", obj);

                foreach (PropertyInfo pi in GetReportableProperties(obj.GetType()))
                {
                  object left = pi.GetValue(original, null);
                  object right = pi.GetValue(obj, null);
                  if ((left == null && right == null) || (left != null && left.Equals(right)))
                  {
                    //   report += string.Format("{0}: unchanged<br/>", pi.Name);
                  }
                  else
                  {
                    report += string.Format("{0}: {1} => {2}<br/>", pi.Name, left, right);
                  }
                }
                changes.Add(new AuditLog
                {
                  Action = entry.State.ToString(),
                  Comment = report,
                  Collection = entry.EntitySet.Name,
                  Changed = DateTime.Now,
                  ObjectId = obj.Id,
                  User = Thread.CurrentPrincipal.Identity.Name
                });
              }
            }
          }
        }
      }

      // Added and modified objects - we need to fetch more data before we can report what the change was in readable form.
      foreach (ObjectStateEntry entry in osm.GetObjectStateEntries(EntityState.Deleted))
      {
        IModelObject modelObject = GetOriginalVersion(comparisonContext, entry);
        if (modelObject != null)
        {
          Document d = modelObject as Document;
          if (d != null && !string.IsNullOrWhiteSpace(d.StorePath))
          {
            string path = Document.StorageRoot + d.StorePath;
            System.IO.File.Delete(path);
            for (int i = 0; i < Document.StorageTreeDepth; i++)
            {
              path = System.IO.Path.GetDirectoryName(path);
              if (System.IO.Directory.GetDirectories(path).Length + System.IO.Directory.GetFiles(path).Length == 0)
              {
                System.IO.Directory.Delete(path);
              }
            }
          }
          changes.Add(new AuditLog
          {
            Action = entry.State.ToString(),
            Comment = modelObject.GetReportHtml(),
            Collection = entry.EntitySet.Name,
            Changed = DateTime.Now,
            ObjectId = modelObject.Id,
            User = Thread.CurrentPrincipal.Identity.Name
          });
        }
      }

      if (errors.Count > 0)
      {
        throw new RuleViolationsException(errors);
      }

      changes.ForEach(f => this.AuditLog.Add(f));

      return base.SaveChanges();
      //if (SavedChanges != null)
      //{
      //    SavedChanges(this, new SavingChangesArgs { Changes = changes.Select(f => f.Comment).ToList() });
      //}
    }

    private IEnumerable<PropertyInfo> GetReportableProperties(Type forType)
    {
      if (!this.reportingProperties.ContainsKey(forType))
      {
        var properties = forType.GetProperties().ToDictionary(f => f.Name, f => f);
        foreach (string prop in properties.Keys.ToArray())
        {
          if (!properties.ContainsKey(prop)) continue;

          object[] reporting = properties[prop].GetCustomAttributes(typeof(ReportingAttribute), false);
          if (prop == "LastChanged" || prop == "ChangedBy" || prop == "Id")
          {
            properties.Remove(prop);
          }
          else if (reporting.Length == 1)
          {
            // Keep this property in the list

            // Hide the ones it hides...
            ReportingAttribute attrib = (ReportingAttribute)reporting[0];
            this.reportingFormats.Add(forType.FullName + ':' + prop, attrib.Format);
            foreach (string pname in attrib.Hides.Split(','))
            {
              properties.Remove(pname);
            }
          }
          else if (properties[prop].GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Length == 0)
          {
            // Only show ones that have the EdmScalar attribute or the reporting attribute
            properties.Remove(prop);
          }
        }
        this.reportingProperties.Add(forType, properties.Values.ToList());

        foreach (MemberReportingAttribute attrib in forType.GetCustomAttributes(typeof(MemberReportingAttribute), true))
        {
          this.reportingFormats.Add(forType.FullName + ':' + attrib.Property, attrib.Format);
        }
      }
      return this.reportingProperties[forType];
    }

    private IModelObject GetOriginalVersion(KcsarContext context, ObjectStateEntry entry)
    {
      object original;
      if (entry.EntityKey == null || !(entry.Entity is IModelObject))
      {
        // We don't know how to report objects that aren't IModelObjects
        return null;
      }

      // Try to get the original version of the deleted object.
      EntityKey key = new EntityKey(entry.EntityKey.EntityContainerName + "." + entry.EntityKey.EntitySetName, "Id", ((IModelObject)entry.Entity).Id);
      ((IObjectContextAdapter)context).ObjectContext.TryGetObjectByKey(key, out original);

      foreach (var property in original.GetType().GetProperties())
      {
        foreach (ReportedReferenceAttribute attrib in property.GetCustomAttributes(typeof(ReportedReferenceAttribute), true))
        {
          var reference = context.Entry(original).Reference(property.Name);
          if (!reference.IsLoaded) reference.Load();
        }
      }

      // Now that we have the object before it (and its associations) was deleted, we can report on what it was...
      return original as IModelObject;
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

              var missions = (from r in this.Missions.SelectMany(f => f.Responders) where r.MemberId == m.Id && r.Mission.StartTime < time select r);
              if (missionType != "%")
              {
                missions = missions.Where(x => x.Mission.MissionType.Contains(missionType));
              }
              missions = missions.OrderByDescending(x => x.Mission.StartTime);

              decimal sum = 0;
              DateTime startDate = DateTime.Now;
              foreach (MissionResponder response in missions)
              {
                if (response.Role != MissionRoster.ROLE_IN_TOWN && response.Role != MissionRoster.ROLE_NO_ROLE)
                {
                  startDate = response.Mission.StartTime;
                  sum += response.Hours ?? (decimal)0;

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
