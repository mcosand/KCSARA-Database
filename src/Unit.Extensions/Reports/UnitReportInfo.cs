/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Extensions.Reports
{
  using System.Collections.Generic;
  public class UnitReportInfo
  {
    public UnitReportInfo()
    {
      this.Parameters = new Dictionary<string, string>();
    }

    public string Key { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string MimeType { get; set; }

    public Dictionary<string, string> Parameters { get; set; }
  }
}
