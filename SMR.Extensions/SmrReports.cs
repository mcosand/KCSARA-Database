/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Extensions.Reports;
  using OfficeOpenXml;

  public partial class SmrReports : IUnitReports
  {
    const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    readonly Lazy<IKcsarContext> db;

    static readonly Dictionary<string, Action<SmrReports, ExcelPackage>> reportBuilders = new Dictionary<string, Action<SmrReports, ExcelPackage>>
    {
      {"unitRoster", UnitRoster },
      {"fieldSummary", FieldSummary }
    };

    public SmrReports(Lazy<IKcsarContext> db)
    {
      this.db = db;
    }

    public UnitReportInfo[] ListReports()
    {
      return new[]
      {
        new UnitReportInfo { Key = "unitRoster", Name = "SMR Unit Roster", MimeType = XlsxMime, Extension = "xlsx" },
        new UnitReportInfo { Key = "fieldSummary", Name = "SMR Field Member Status Summary", MimeType = XlsxMime, Extension = "xlsx" }
      };
    }

    public void RunReport(string key, Stream stream)
    {
      var info = ListReports().FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));

      ExcelPackage package;
      using (var templateStream = typeof(SmrReports).Assembly.GetManifestResourceStream("SMR.Extensions.templates." + info.Key + ".xlsx"))
      {
        package = new ExcelPackage(templateStream);
      }

      reportBuilders[info.Key](this, package);

      package.SaveAs(stream);
      package.Dispose();
    }
  }
}
