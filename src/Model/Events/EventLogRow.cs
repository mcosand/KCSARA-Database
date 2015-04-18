/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsar.Database.Data.Events
{
  using System.ComponentModel.DataAnnotations.Schema;
  using Newtonsoft.Json;

  // Root table: EventTimelines
  public class EventLogRow : ParticipantEventTimeline
  {
    [NotMapped]
    public string Message
    {
      get { return JsonConvert.DeserializeObject<string>(this.JsonData); }
      set { this.JsonData = JsonConvert.SerializeObject(value); }
    }

    public override string ToMarkdown()
    {
      return this.Message;
    }

    public override string GetReportHtml()
    {
      return string.Format("<b>{0}</b> @{1}: {2}", this.Event.Title, this.Time, this.Message);
    }
  }
}
