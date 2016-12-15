using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsar.Database.Model
{

  public class AnimalMission : ModelObject
  {
    public const string ReportFormat = "<b>[{0}] [{1}]</b> In:{2} Out:{3}";

    [Column("Animal_Id")]
    public Guid AnimalId { get; set; }
    [ForeignKey("AnimalId")]
    public virtual Animal Animal { get; set; }

    [Column("MissionRoster_Id")]
    public Guid MissionRosterId { get; set; }
    [ForeignKey("MissionRosterId")]
    public virtual MissionRoster MissionRoster { get; set; }

    public override string GetReportHtml()
    {
      return string.Format(AnimalMission.ReportFormat, this.MissionRoster.Mission.Title, this.Animal.Name, this.MissionRoster.TimeIn, this.MissionRoster.TimeOut);
    }
  }
}
