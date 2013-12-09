
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public struct TrainingStatus
    {
        public DateTime? Expires;
        public ExpirationFlags Status;
        public DateTime? Completed;
        public string CourseName;
        public Guid CourseId;

        public override string ToString()
        {
            return this.Expires.HasValue
                        ? this.Expires.Value.ToString("yyyy-MM-dd")
                        : (this.Status == ExpirationFlags.NotNeeded) ? "" : this.Status.ToString();
        }
    }

    [Flags]
    public enum ExpirationFlags
    {
        Unknown = 0,
        Okay = 1,
        NotNeeded = 5, // 4 + 1
        Complete = 9, // 8 + 1
        NotExpired = 17, // 16 + 1
        Expired = 32,
        Missing = 64,
    }

    public class CompositeTrainingStatus
    {
        public IDictionary<Guid, TrainingStatus> Expirations { get; private set; }

        public bool IsGood { get; private set; }

        private CompositeTrainingStatus()
        {
            this.IsGood = true;
            this.Expirations = new Dictionary<Guid, TrainingStatus>();
        }

        public static CompositeTrainingStatus Compute(Member m, IEnumerable<TrainingCourse> courses, DateTime when)
        {
            //if (!m.ComputedAwards.IsLoaded)
            //{
            //    throw new InvalidOperationException("Must pre-load member's .ComputedAwards.Course");
            //}
            
            return CompositeTrainingStatus.Compute(m, m.ComputedAwards, courses, when);
        }

        public static CompositeTrainingStatus Compute(Member m, IEnumerable<ComputedTrainingAward> awards, IEnumerable<TrainingCourse> courses, DateTime when)
        {
            CompositeTrainingStatus result = new CompositeTrainingStatus();

            foreach (TrainingCourse course in courses)
            {
                TrainingStatus status = new TrainingStatus { CourseId = course.Id, CourseName = course.DisplayName, Expires = null, Status = ExpirationFlags.Unknown };

                bool mustHave = m.IsTrainingRequired(course);
                bool keepCurrent = m.ShouldKeepCourseCurrent(course);
                ComputedTrainingAward award = awards.Where(f => f.Course.Id == course.Id).FirstOrDefault();
                if (award == null)
                {
                    // No record - member has not completed the training
                    status.Status = mustHave ? ExpirationFlags.Missing : ExpirationFlags.NotNeeded;
                }
                else if ((course.ValidMonths ?? 0) == 0)
                {
                    // A record without an expiration
                    status.Status = ExpirationFlags.Complete;
                    status.Completed = award.Completed;
                }
                else
                {
                    // A record that has an expiration

                    status.Expires = award.Expiry;
                    status.Completed = award.Completed;
                    status.Status = (mustHave && keepCurrent)
                                     ? ((award.Expiry < when) ? ExpirationFlags.Expired : ExpirationFlags.NotExpired)
                                     : ExpirationFlags.Complete;
                }
                result.Expirations.Add(course.Id, status);

                // Ugh. Now we get to perpetuate a hack for a change in requirements for crime scene with a period when we ignored expired crime scene.
                if (!(course.DisplayName == "Crime Scene" && status.Expires > new DateTime(2007, 03, 01) && when > new DateTime(2010, 6, 3) && when < new DateTime(2010, 11, 12)))
                result.IsGood = result.IsGood && ((status.Status & ExpirationFlags.Okay) == ExpirationFlags.Okay);
            }
            return result;
        }
    }
}