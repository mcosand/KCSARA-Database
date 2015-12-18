/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.IO;
  using System.Linq;
  using Kcsar.Database.Model;
  using Kcsara.Database.Extensions.Reports;
  using OfficeOpenXml;

  public partial class SmrReports : IUnitReports
  {
    const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    readonly Lazy<IKcsarContext> db;

    static readonly Dictionary<string, Action<SmrReports, ExcelPackage, NameValueCollection>> reportBuilders =
      new Dictionary<string, Action<SmrReports, ExcelPackage, NameValueCollection>>
    {
      {"unitRoster", UnitRoster },
      {"fieldSummary", FieldSummary },
      {"trainingList", TrainingList },
      {"missionList", MissionList },
      {"missionRosters", MissionRosters }
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
        new UnitReportInfo { Key = "fieldSummary", Name = "SMR Field Member Status Summary", MimeType = XlsxMime, Extension = "xlsx" },
        new UnitReportInfo { Key = "trainingList", Name = "SMR Unit Training List", MimeType = XlsxMime, Extension = "xlsx" },
        new UnitReportInfo { Key = "missionList", Name = "SMR Mission List", MimeType = XlsxMime, Extension = "xlsx", Parameters = new Dictionary<string,string> { { "year", DateTime.Today.Year.ToString() } } },
        new UnitReportInfo { Key = "missionRosters", Name = "SMR Mission Rosters", MimeType = XlsxMime, Extension = "xlsx", Parameters = new Dictionary<string,string> { { "year", DateTime.Today.Year.ToString() } } }
      };
    }

    public void RunReport(string key, Stream stream, NameValueCollection queries)
    {
      var info = ListReports().FirstOrDefault(f => string.Equals(f.Key, key, StringComparison.OrdinalIgnoreCase));

      ExcelPackage package;
      using (var templateStream = typeof(SmrReports).Assembly.GetManifestResourceStream("SMR.Extensions.templates." + info.Key + ".xlsx"))
      {
        package = new ExcelPackage(templateStream);
      }

      reportBuilders[info.Key](this, package, queries);

      package.SaveAs(stream);
      package.Dispose();
    }

    private static MissionInfo[] GetMissions(IQueryable<MissionRoster_Old> rosters)
    {
      return rosters.Select(f => f.Mission).Distinct().Select(f => new MissionInfo
      {
        Id = f.Id,
        StartTime = f.StartTime,
        StateNumber = f.StateNumber,
        Title = f.Title,
        Location = f.Location,
        MissionType = f.MissionType
      }).OrderByDescending(f => f.StartTime).ToArray();
    }

    private static IQueryable<MissionRoster_Old> GetMissionRostersQuery(SmrReports me, DateTime start, DateTime stop)
    {
      return me.db.Value.MissionRosters.Where(f => f.Unit.DisplayName == "SMR" && f.Mission.StartTime >= start && f.Mission.StartTime < stop);
    }
  }
}
