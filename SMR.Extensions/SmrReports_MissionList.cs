/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Collections.Specialized;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using OfficeOpenXml;
  using OfficeOpenXml.Style;

  public partial class SmrReports
  {
    private static void MissionList(SmrReports me, ExcelPackage package, NameValueCollection entries)
    {
      var sheet = package.Workbook.Worksheets[1];

      int year;
      if (!int.TryParse(entries["year"], out year))
      {
        throw new InvalidOperationException("requires argument 'year'");
      }

      DateTime start = new DateTime(year, 1, 1);
      DateTime stop = new DateTime(year + 1, 1, 1);

      var rosters = me.db.Value.MissionRosters.Where(f => f.Unit.DisplayName == "SMR" && f.Mission.StartTime >= start && f.Mission.StartTime < stop);
      var missions = rosters.Select(f => f.Mission).Distinct().Select(f => new
      {
        f.Id,
        f.StartTime,
        f.StateNumber,
        f.Title,
        f.Location,
        f.MissionType
      }).OrderByDescending(f => f.StartTime).ToArray();
      
      var stats = rosters.GroupBy(f => f.Mission.Id)
        .Select(f => new
        {
          Id = f.Key,
          Persons = f.Select(g => g.Person.Id).Distinct().Count(),
          Hours = f.Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 60.0),
          Miles = f.Sum(g => g.Miles)
        })
        .ToDictionary(f => f.Id, f => f);

      var total = rosters
        .GroupBy(f => 1)
        .Select(f => new
        {
          Persons = f.Select(g => g.Person.Id).Distinct().Count(),
          Hours = f.Sum(g => SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut) / 60.0),
          Miles = f.Sum(g => g.Miles)
        })
        .Single();

      sheet.Cells[1, 1].Value += string.Format(" ({0})", year);

      for (int i = 0; i < missions.Length; i++)
      {
        var col = 1;
        var row = i + 4;
        sheet.Cells[row, col++].Value = missions[i].StartTime;
        sheet.Cells[row, col++].Value = missions[i].StateNumber;
        sheet.Cells[row, col++].Value = string.Format("{0}{1}", missions[i].Title, (missions[i].MissionType ?? string.Empty).Contains("turnaround") ? " (Turnaround)" : string.Empty);
        sheet.Cells[row, col++].Value = stats[missions[i].Id].Persons;
        sheet.Cells[row, col++].Value = stats[missions[i].Id].Hours;
        sheet.Cells[row, col++].Value = stats[missions[i].Id].Miles;
      }
      sheet.Row(missions.Length + 4).Style.Border.Top.Style = ExcelBorderStyle.Thin;
      sheet.Row(missions.Length + 4).Style.Border.Top.Color.SetColor(System.Drawing.Color.FromArgb(0, 76, 154));
      sheet.Row(missions.Length + 4).Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 76, 154));
      sheet.Row(missions.Length + 4).Style.Font.Bold = true;
      sheet.Cells[missions.Length + 4, 3].Value = "Total Missions = " + missions.Length;
      sheet.Cells[missions.Length + 4, 4].Value = total.Persons;
      sheet.Cells[missions.Length + 4, 5].Value = total.Hours;
      sheet.Cells[missions.Length + 4, 6].Value = total.Miles;
      sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }
  }
}
