/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Collections.Specialized;
  using System.Drawing;
  using System.Linq;
  using Kcsar.Database.Model;
  using OfficeOpenXml;
  using OfficeOpenXml.Style;

  public partial class SmrReports
  {
    private static void MissionRosters(SmrReports me, ExcelPackage package, NameValueCollection entries)
    {
      var sheet = package.Workbook.Worksheets[1];

      int year;
      if (!int.TryParse(entries["year"], out year))
      {
        throw new InvalidOperationException("requires argument 'year'");
      }

      DateTime start = new DateTime(year, 1, 1);
      DateTime stop = new DateTime(year + 1, 1, 1);

      IQueryable<MissionRoster_Old> rosterQuery = GetMissionRostersQuery(me, start, stop);
      MissionInfo[] missions = GetMissions(rosterQuery);

      var rosters = rosterQuery
        .GroupBy(f => f.Mission.Id)
        .ToDictionary(
          f => f.Key,
          f => f.GroupBy(g => new { g.Person.DEM, g.Person.LastName, g.Person.FirstName, g.Person.Id, g.InternalRole })
                .ToDictionary(
                  g => g.Key,
                  g => new
                  {
                    Hours = g.Sum(h => (h.TimeOut - h.TimeIn).Value.TotalHours /*SqlFunctions.DateDiff("minute", h.TimeIn, h.TimeOut) / 60.0*/),
                    Miles = g.Sum(h => h.Miles)
                  }));

      sheet.Cells[1, 1].Value += string.Format(" ({0})", year);

      var row = 4;
      for (int i = 0; i < missions.Length; i++)
      {
        sheet.Cells[row, 1].Value = string.Format("{0:yyyy-MM-dd}  ({1}) {2}{3}", missions[i].StartTime, missions[i].StateNumber, missions[i].Title, (missions[i].MissionType ?? string.Empty).Contains("turnaround") ? " (Turnaround)" : string.Empty);
        sheet.Row(row).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        sheet.Row(row).Merged = true;
        sheet.Row(row).Style.Font.Bold = true;
        sheet.Row(row).Style.Border.Top.Style = ExcelBorderStyle.Thin;
        sheet.Row(row).Style.Border.Top.Color.SetColor(Color.FromArgb(0, 76, 154));
        row++;

        var roster = rosters[missions[i].Id];
        foreach (var r in roster.OrderBy(f => f.Key.LastName).ThenBy(f => f.Key.FirstName).ThenBy(f => f.Key.Id))
        {
          sheet.Cells[row, 1].Value = r.Key.DEM;
          sheet.Cells[row, 2].Value = string.Format("{0}, {1}", r.Key.LastName, r.Key.FirstName);
          sheet.Cells[row, 3].Value = r.Key.InternalRole;
          sheet.Cells[row, 4].Value = r.Value.Hours;
          sheet.Cells[row, 5].Value = r.Value.Miles;
          row++;
        }

        sheet.Cells[row, 2].Value = "Total Personnel = " + roster.Select(f => f.Key.Id).Distinct().Count();
        sheet.Cells[row, 4].Value = roster.Sum(f => f.Value.Hours);
        sheet.Cells[row, 5].Value = roster.Sum(f => f.Value.Miles);
        sheet.Row(row).Style.Font.Color.SetColor(Color.FromArgb(0, 76, 154));
        sheet.Row(row).Height *= 1.75;
        sheet.Row(row).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        row++;
      }
    }
  }
}
