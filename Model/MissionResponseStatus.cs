/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsar.Database.Model
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.RegularExpressions;

  public class MissionResponseStatus : ModelObject
  {
    public DateTime CallForPeriod { get; set; }
    public DateTime? StopStaging { get; set; }

    public virtual Mission Mission { get; set; }

    public override string ToString()
    {
      return string.Format("Response Status for {0}", this.Mission);
    }

    public override string GetReportHtml()
    {
      return string.Format("Response Status of [<b>{0}</b>]", this.Mission.Title);
    }

    #region IValidatedEntity Members

    public override bool Validate()
    {
      errors.Clear();

      return (errors.Count == 0);
    }

    #endregion
  }
}
