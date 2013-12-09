
namespace Kcsar.Database.Model
{
    using System;
    using System.Collections.Generic;

    public class TrainingRoster : ModelObject, IRosterEntry<Training, TrainingRoster>
    {
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public int? Miles { get; set; }
        public string Comments { get; set; }
        public virtual Member Person { get; set; }
        public virtual Training Training { get; set; }
        public virtual ICollection<TrainingAward> TrainingAwards { get; set; }
        public virtual ICollection<ComputedTrainingAward> ComputedAwards { get; set; }
        public int? OvertimeHours { get; set; }

        public TrainingRoster()
        {
            this.TrainingAwards = new List<TrainingAward>();
            this.ComputedAwards = new List<ComputedTrainingAward>();
        }

        public double? Hours
        {
            get
            {
                if (this.TimeIn == null || this.TimeOut == null)
                {
                    return null;
                }

                return (this.TimeOut - this.TimeIn).Value.TotalHours;
            }
        }

        bool timeInDirty = false;
        bool timeOutDirty = false;

        //partial void OnTimeInChanging(DateTime? value)
        //{
        //    timeInDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.TimeIn);
        //}

        //partial void OnTimeOutChanging(DateTime? value)
        //{
        //    timeOutDirty = (this.EntityState != System.Data.EntityState.Detached && value != this.TimeOut);
        //}

        public IRosterEvent<Training, TrainingRoster> GetRosterEvent()
        {
            return this.Training;
        }

        public Func<TrainingRoster, Training> RosterToEventFunc
        {
            get { return x => x.Training; }
        }

        public IRosterEvent GetEvent()
        {
            return this.Training;
        }

        public void SetEvent(IRosterEvent sarEvent)
        {
            // Passing a non-training IRosterEvent here will (and should) throw a cast exception.
            this.Training = (Training)sarEvent;
        }

        public override bool Validate()
        {
            errors.Clear();

            //if (this.Person == null && this.PersonReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Person", "", "Required"));

            //    //  Other validations will fail when this reference is null. Short circuit here.
            //    return false;
            //}

            //if (this.Training == null && this.TrainingReference.EntityKey == null)
            //{
            //    errors.Add(new RuleViolation(this.Id, "Training", "", "Required"));
            //}

            ValidateTimeInOut();
            if (this.Miles.HasValue && this.Miles.Value < 0)
            {
                errors.Add(new RuleViolation(this.Id, "Miles", this.Miles.Value.ToString(), "Invalid"));
            }

            return (errors.Count == 0);
        }

        private bool ValidateTimeInOut()
        {
            bool result = true;

            if (!this.TimeIn.HasValue)
            {
                errors.Add(new RuleViolation(this.Id, "TimeIn", "", "Time In is required"));
                return false;
            }

            if (this.TimeOut.HasValue)
            {
                string field = (timeInDirty && !timeOutDirty) ? "TimeIn" : "TimeOut";
                string badValue = ((timeInDirty && !timeOutDirty) ? this.TimeIn.Value : this.TimeOut.Value).ToString("MM/dd/yy HH:mm");

                if (this.TimeOut <= this.TimeIn)
                {
                    errors.Add(new RuleViolation(this.Id, field, badValue, "In must be before Out"));
                    return false;
                }

                // Load the Person again, along with all the Training rosters, so we can search through them...

                //  InA    InThis  OutThis    OutA
                //  InA    InThis   OutA    OutThis
                //  InThis   InA    OutThis    OutA
                //  InThis   InA    OutA     OutThis


                //if (this.Person == null && !this.PersonReference.IsLoaded)
                //{
                //    this.PersonReference.Load();
                //}

                //if (!this.Person.TrainingRosters.IsLoaded)
                //{
                //    this.Person.TrainingRosters.Load();
                //}

                foreach (TrainingRoster row in this.Person.TrainingRosters)
                {
                    if (row.Id == this.Id || !row.TimeOut.HasValue/* || row.EntityState == System.Data.EntityState.Deleted*/)
                    {
                        continue;
                    }

                    if ((row.TimeIn <= this.TimeIn && row.TimeOut.Value > this.TimeIn) || (this.TimeIn <= row.TimeIn && this.TimeOut.Value > row.TimeIn))
                    {
                        errors.Add(new RuleViolation(this.Id, field, badValue,
                            string.Format("Conflicts with another roster entry, where TimeIn={0} and TimeOut={1}", row.TimeIn, row.TimeOut)));
                        result = false;
                    }
                }
            }
            return result;
        }

        private void ClearErrors(string property)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                if (errors[i].PropertyName == property)
                {
                    errors.RemoveAt(i);
                    i--;
                }
            }
        }

        public override string GetReportHtml()
        {
            //if (this.EntityState == System.Data.EntityState.Modified || this.EntityState == System.Data.EntityState.Unchanged)
            //{
            //    if (!this.PersonReference.IsLoaded)
            //    {
            //        this.PersonReference.Load();
            //    }

            //    if (!this.TrainingReference.IsLoaded)
            //    {
            //        this.TrainingReference.Load();
            //    }
            //}
            return string.Format("[<b>{0}</b>] [<b>{1}</b>] In:{2}, Out:{3}, Miles:{4}", this.Training.Title, this.Person.FullName, this.TimeIn, this.TimeOut, this.Miles);
        }

        public override string ToString()
        {
            return this.GetReportHtml();
        }
    }
}
