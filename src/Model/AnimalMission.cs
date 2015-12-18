/*
 * Copyright 2009-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{

  public class AnimalMission : ModelObject
  {
    public const string ReportFormat = "<b>[{0}] [{1}]</b> In:{2} Out:{3}";

    public virtual Animal Animal { get; set; }
    public virtual MissionRoster_Old MissionRoster { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalMission.ReportFormat, this.MissionRoster.Mission.Title, this.Animal.Name, this.MissionRoster.TimeIn, this.MissionRoster.TimeOut);
    }
  }
}
