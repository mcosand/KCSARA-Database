/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Collections.Specialized;
  using System.Linq;
  using Kcsar.Database.Model;
  using OfficeOpenXml;

  public partial class SmrReports
  {
    private static void FieldSummary(SmrReports me, ExcelPackage package, NameValueCollection queries)
    {
      var sheet = package.Workbook.Worksheets[1];
      
      var memberships = me.db.Value.Units
        .Where(f => f.DisplayName == "SMR")
        .SelectMany(f => f.Memberships.Where(g => (g.EndTime == null || g.EndTime > DateTime.Now) && g.Status.IsActive && g.Status.WacLevel != WacLevel.None));

      var members = memberships
        .Select(f => f.Person);

      var memberHistory = members.ToDictionary(f => f.Id, f => f.Memberships.Where(g => g.Status.IsActive && g.Unit.DisplayName == "SMR").OrderByDescending(g => g.Activated).ToList());

      var currentTraining = members.SelectMany(f => f.ComputedAwards)
        .GroupBy(f => f.Member.Id)
        .ToDictionary(f => f.Key, f => f.Select(g => new { g.Course.DisplayName, g.NullableCompleted }).ToArray());

      var today = DateTime.Today;
      var oneYear = today.AddYears(-1);
      var threeYears = today.AddYears(-3);

      var recentMissions = members.SelectMany(f => f.MissionRosters)
        .Where(f => f.Mission.StartTime > threeYears && f.Unit.DisplayName == "SMR")
        .GroupBy(f => f.Person.Id, f => f)
        .ToDictionary(
          f => f.Key,
          f => f.GroupBy(g => g.Mission.StartTime, g => g)
                .ToDictionary(g => g.Key, g => new
                {
                  Hours = g.Sum(h => h.TimeOut.HasValue ? (h.TimeOut.Value - h.TimeIn.Value).TotalHours : (double?)null),
                  Count = g.Select(h => h.Mission.Id).Distinct().Count()
                }));

      var recentTrainings = members.SelectMany(f => f.TrainingRosters)
        .Where(f => f.Training.StartTime > threeYears)
        .GroupBy(f => f.Person.Id, f => f)
        .ToDictionary(
          f => f.Key,
          f => f.GroupBy(g => g.Training.StartTime, g => g)
                .ToDictionary(g => g.Key, g => new
                {
                  Hours = g.Sum(h => h.TimeOut.HasValue ? (h.TimeOut.Value - h.TimeIn.Value).TotalHours : (double?)null),
                  Count = g.Select(h => h.Training.Id).Distinct().Count()
                }));

      var recentRigging = members.SelectMany(f => f.TrainingRosters)
        .Where(f => f.Training.StartTime > threeYears && f.TrainingAwards.Any(g => g.Course.DisplayName == "SMR Rigging Refresher"))
        .GroupBy(f => f.Person.Id, f => f)
        .ToDictionary(
          f => f.Key,
          f => f.GroupBy(g => g.Training.StartTime, g => g)
                .ToDictionary(g => g.Key, g => new
                {
                  Hours = g.Sum(h => h.TimeOut.HasValue ? (h.TimeOut.Value - h.TimeIn.Value).TotalHours : (double?)null),
                  Count = g.Select(h => h.Training.Id).Distinct().Count()
                }));



      var status = memberships.ToDictionary(f => f.Person.Id, f => new { f.Status.StatusName, f.Activated });
      
      var roster = members.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ThenBy(f => f.Id).ToArray();

      var oldDate = new DateTime(1930, 1, 1);

      for (int i = 0; i < roster.Length; i++)
      {
        var memberId = roster[i].Id;
        var col = 1;
        var row = i + 4;

        var joinDate = (DateTime?)null;
        var joinHistory = memberHistory[memberId];
        for (int j = 0; j < joinHistory.Count; j++)
        {
          if (j == 0 || joinDate.Value.Date == joinHistory[j].EndTime.Value.Date)
          {
            joinDate = joinHistory[j].Activated;
          }
          else
          {
            break;
          }
        }

        sheet.Cells[row, col++].Value = roster[i].DEM;
        sheet.Cells[row, col++].Value = roster[i].ReverseName;
        sheet.Cells[row, col++].Value = joinDate > oldDate ? joinDate : (DateTime?)null;
        sheet.Cells[row, col++].Value = roster[i].WacLevel == WacLevel.None ? null : roster[i].WacLevel.ToString();
        sheet.Cells[row, col++].Value = status[memberId].StatusName;
        col++; // MRA

        if (currentTraining.ContainsKey(memberId) && currentTraining[memberId] != null)
        {
          sheet.Cells[row, col++].Value = currentTraining[memberId].Any(f => f.DisplayName == "Avalanche III") ? "Avy III"
                                        : currentTraining[memberId].Any(f => f.DisplayName == "Avalanche II") ? "Avy II"
                                        : currentTraining[memberId].Any(f => f.DisplayName == "Avalanche I") ? "Avy I"
                                        : null;
          sheet.Cells[row, col++].Value = currentTraining[memberId].Any(f => f.DisplayName == "SMR Snowmobile II") ? "Advanced"
                                        : currentTraining[memberId].Any(f => f.DisplayName == "SMR Snowmobile") ? "Basic"
                                        : null;
        }
        else
        {
          col += 2;
        }

        sheet.Cells[row, col++].Value = recentTrainings.ContainsKey(memberId) ? recentTrainings[memberId].Sum(f => f.Value.Count) : (int?)null;
        sheet.Cells[row, col++].Value = recentMissions.ContainsKey(memberId) ? recentMissions[memberId].Where(f => f.Key > oneYear).Sum(f => f.Value.Count) : (int?)null;
        sheet.Cells[row, col++].Value = recentMissions.ContainsKey(memberId) ? recentMissions[memberId].Sum(f => f.Value.Count) : (int?)null;
        sheet.Cells[row, col++].Value = recentMissions.ContainsKey(memberId) ? (int?)recentMissions[memberId].Sum(f => f.Value.Hours) : null;
        sheet.Cells[row, col++].Value = recentRigging.ContainsKey(memberId) ? (int?)recentRigging[memberId].Where(f => f.Key > oneYear).Sum(f => f.Value.Hours) : null;
        sheet.Cells[row, col++].Value = recentRigging.ContainsKey(memberId) ? (int?)recentRigging[memberId].Sum(f => f.Value.Hours) : null;
        sheet.Cells[row, col++].Value = recentTrainings.ContainsKey(memberId) ? (int?)recentTrainings[memberId].Sum(f => f.Value.Hours) : null;
      }
      sheet.Cells["A:C"].AutoFitColumns();
      sheet.Column(3).Width += 2;
    }
  }
}
