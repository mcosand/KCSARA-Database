using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kcsar.Database.Model
{
  public class TrainingRule : ModelObject
  {
    public string RuleText { get; set; }
    public DateTime? OfferedFrom { get; set; }
    public DateTime? OfferedUntil { get; set; }
    public virtual ICollection<TrainingAward> Results { get; set; }

    [ForeignKey("UnitId")]
    public SarUnit Unit { get; set; }
    public Guid? UnitId { get; set; }

    public override string GetReportHtml()
    {
      return RuleText;
    }
  }
}
