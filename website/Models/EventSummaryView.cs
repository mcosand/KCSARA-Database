/*
 * Copyright 2010-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Model
{
    using System.Runtime.Serialization;
    using Kcsar.Database.Model;

    [DataContract]
    public class EventSummaryView
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }


        // Optionally populated
        [DataMember(EmitDefaultValue=false)]
        public bool IsActive { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsTurnaround { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Persons { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public double? Hours { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? Miles { get; set; }

        public EventSummaryView() { }

        public EventSummaryView(IRosterEvent sarEvent)
        {
            this.Id = sarEvent.Id;
            this.Title = sarEvent.Title;
            this.Number = sarEvent.StateNumber;
            this.StartTime = sarEvent.StartTime;
        }
    }

    [DataContract]
    public class EventReportStatusView : EventSummaryView
    {
        [DataMember]
        public IEnumerable<string> Units { get; set; }

        [DataMember]
        public int GeoCount { get; set; }

        [DataMember]
        public int LogCount { get; set; }

        [DataMember]
        public int SubjectCount { get; set; }

        [DataMember]
        public int DocumentCount { get; set; }

        [DataMember]
        public int NotSignedOut { get; set; }
    }
}
