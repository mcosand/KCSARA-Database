/*
 * Copyright 2015 Matthew Cosand
 */
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kcsar.Database.Model.Events
{
  public class EventLog : ParticipantEventTimeline
  {
    [NotMapped]
    public string Message
    {
      get { return JsonConvert.DeserializeObject<string>(this.JsonData); }
      set { this.JsonData = JsonConvert.SerializeObject(value); }
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> @{1}: {2}", this.Event.Title, this.Time, this.Message);
    }
  }
}
