namespace ESAR.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.IO;
  using System.Linq;
  using Kcsar.Database.Model;
  using OfficeOpenXml;
  using Sar.Database.Api.Extensions;

  public partial class EsarReports : IUnitReports
  {
    const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    readonly Lazy<IKcsarContext> db;

    static readonly Dictionary<string, Action<EsarReports, ExcelPackage, NameValueCollection>> reportBuilders =
      new Dictionary<string, Action<EsarReports, ExcelPackage, NameValueCollection>>
    {
        {"trainingReport", TrainingReport }
    };

    public EsarReports(Lazy<IKcsarContext> db)
    {
      this.db = db;
    }

    public UnitReportInfo[] ListReports()
    {
      return new[]
      {
        new UnitReportInfo { Key = "trainingReport", Name = "ESAR Training Report", MimeType = XlsxMime, Extension = "xlsx" }
      };
    }

    public void RunReport(string key, Stream stream, NameValueCollection queries)
    {
      var info = ListReports().FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));

      ExcelPackage package;
      using (var templateStream = typeof(EsarReports).Assembly.GetManifestResourceStream("ESAR.Extensions.templates." + info.Key + ".xlsx"))
      {
        package = new ExcelPackage(templateStream);
      }

      reportBuilders[info.Key](this, package, queries);

      package.SaveAs(stream);
      package.Dispose();
    }
  }
}
