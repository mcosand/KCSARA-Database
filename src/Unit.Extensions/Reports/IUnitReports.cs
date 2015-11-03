/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Extensions.Reports
{
  using System.Collections.Specialized;
  using System.IO;

  [ExtensionInterface]
  public interface IUnitReports
  {
    UnitReportInfo[] ListReports();
    void RunReport(string key, Stream stream, NameValueCollection queryString);
  }
}
