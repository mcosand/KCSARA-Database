/*
 * Copyright 2012-2015 Matthew Cosand
 */
namespace Kcsar.Database.Data
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;

  [Table("TrainingRules")]
  public class TrainingRuleRow : ModelObjectRow
  {
    public string RuleText { get; set; }
    public DateTime? OfferedFrom { get; set; }
    public DateTime? OfferedUntil { get; set; }
    public virtual ICollection<ComputedTrainingRecordRow> Results { get; set; }

    public override string GetReportHtml()
    {
      return RuleText;
    }
  }
}
