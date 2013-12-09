
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Training : ModelObject, IRosterEvent<Training, TrainingRoster>
    {
        [Required]
        public string Title { get; set; }
        public string County { get; set; }
        public string StateNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public Guid? Previous { get; set; }
        public string Comments { get; set; }
        public virtual ICollection<TrainingRoster> Roster { get; set; }
        public virtual ICollection<TrainingCourse> OfferedCourses { get; set; }
        [Required]
        public string Location { get; set; }
 

        public Training()
            : base()
        {
            this.County = "King";
            this.StartTime = DateTime.Now.Date;
            this.Roster = new List<TrainingRoster>();
            this.OfferedCourses = new List<TrainingCourse>();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.StateNumber, this.Title);
        }

        bool stopDirty = false;
        bool startDirty = false;

        //partial void OnStartTimeChanging(DateTime value)
        //{
        //    startDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.StartTime);
        //}

        //partial void OnStopTimeChanging(DateTime? value)
        //{
        //    stopDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.StopTime);
        //}

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //}
            return string.Format("<b>{0}</b> Start:{1}, County:{2}, Completed:{3}", this.Title, this.StartTime, this.StateNumber, this.StopTime);
        }

        #region IValidatedEntity Members


        public override bool Validate()
        {
            errors.Clear();

            if (string.IsNullOrEmpty(this.Title))
            {
                errors.Add(new RuleViolation(this.Id, "Title", "", "Required"));
            }

            if (string.IsNullOrEmpty(this.Location))
            {
                errors.Add(new RuleViolation(this.Id, "Location", "", "Required"));
            }

            if (string.IsNullOrEmpty(this.County))
            {
                errors.Add(new RuleViolation(this.Id, "County", "", "Required"));
            }
            else if (!Utils.CountyNames.Contains(this.County, new Utils.CaseInsensitiveComparer()))
            {
                errors.Add(new RuleViolation(this.Id, "County", this.County, "Not a county in Washington"));
            }


            if (this.StopTime.HasValue)
            {
                string field = (startDirty && !stopDirty) ? "StartTime" : "StopTime";
                string badValue = ((startDirty && !stopDirty) ? this.StartTime : this.StopTime.Value).ToString("MM/dd/yy HH:mm");

                if (this.StopTime <= this.StartTime)
                {
                    errors.Add(new RuleViolation(this.Id, field, badValue, "Start must be before Stop"));
                }
            }

            if (!string.IsNullOrEmpty(this.StateNumber) && !Regex.IsMatch(this.StateNumber, @"\d{2}\-T-\d{4}"))
            {
                errors.Add(new RuleViolation(this.Id, "StateNumber", this.StateNumber, "Must be in form 00-T-0000"));
            }

            return (errors.Count == 0);
        }

        #endregion


        #region IRosterEvent<Training,TrainingRoster> Members

        IEnumerable<TrainingRoster> IRosterEvent<Training, TrainingRoster>.Roster
        {
            get { return (IEnumerable<TrainingRoster>)this.Roster; }
        }

        #endregion
    }
}
