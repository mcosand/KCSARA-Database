/*
 * Copyright 2015 Matthew Cosand
 */
namespace SMR.Extensions
{
  using System;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using Kcsar.Database.Model;
  using OfficeOpenXml;

  public partial class SmrReports
  {
    private static void TrainingList(SmrReports me, ExcelPackage package)
    {
      var sheet = package.Workbook.Worksheets[1];

      // Find trainings where SMR either hosted, or it was not hosted and an SMR member attended.
      var trainings = me.db.Value.TrainingRosters.Where(
        f => (f.Training.HostUnit.DisplayName == "SMR" || f.Training.HostUnit == null)
        && f.Person.Memberships.Any(h =>
                      h.Status.IsActive
                   && h.Unit.DisplayName == "SMR"
                   && h.Activated <= f.Training.StartTime
                   && (h.EndTime == null || h.EndTime > f.Training.StartTime)))
                   .GroupBy(f => f.Training)
                   .Select(f => new {
                     f.Key.StartTime,
                     f.Key.Title,
                     f.Key.Location,
                     f.Key.StateNumber,
                     Persons = f.Select(g => g.Person.Id).Distinct().Count(),
                     Hours = f.Sum(g => (g.TimeOut.HasValue && g.TimeIn.HasValue) ?  SqlFunctions.DateDiff("minute", g.TimeIn, g.TimeOut)/60.0 : 0.0),
                     Miles = f.Sum(g => g.Miles)
                   })
                   .OrderByDescending(f => f.StartTime)
                   .ToArray();

      for (int i = 0; i < trainings.Length; i++)
      {
        var col = 1;
        var row = i + 4;
        sheet.Cells[row, col++].Value = trainings[i].StartTime;
        sheet.Cells[row, col++].Value = trainings[i].StateNumber;
        sheet.Cells[row, col++].Value = string.Format("{0} - {1}", trainings[i].Title, trainings[i].Location);
        sheet.Cells[row, col++].Value = string.Empty;
        sheet.Cells[row, col++].Value = trainings[i].Persons;
        sheet.Cells[row, col++].Value = trainings[i].Hours;
        sheet.Cells[row, col++].Value = trainings[i].Miles;
      }
      sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }
  }
}
