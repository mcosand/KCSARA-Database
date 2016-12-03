namespace IST.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Kcsar.Database.Model;
  using OfficeOpenXml;

  public partial class IstReports
  {
    private static void TrainingReport(IstReports me, ExcelPackage package, NameValueCollection queries)
    {
      Guid id = new Guid("c118ce30-cd28-4635-ba3d-adf7c21358e2");
      var db = me.db.Value;

      DateTime today = DateTime.Today;
      // Take current month and subtract 1 to move to Jan = 0 counting system
      // Take away one more so that reports run during the first month of a new quarter report on last quarter.
      // Then, convert -1 to 12 with +12,%12
      // Divide by 3 months to get the quarter
      int quarter = ((today.Month + 10) % 12) / 3;
      DateTime quarterStart = new DateTime(today.AddMonths(-1).Year, 1, 1).AddMonths(quarter * 3);
      DateTime quarterStop = quarterStart.AddMonths(3);


      var sheet = package.Workbook.Worksheets[1];

      var members = db.GetActiveMembers(id, today, "ContactNumbers", "Memberships").OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      var istCourses = new[] { "ICS-300", "ICS-400", "ICS-200", "ICS-800" };
      var courses = db.TrainingCourses.Where(f => istCourses.Contains(f.DisplayName) || f.WacRequired > 0).OrderBy(f => f.DisplayName);

      sheet.Cells[1, 1].Value = today;
      sheet.Cells[1, 1].Style.Font.Bold = true;

      var headers = new[] { "Last Name", "First Name", "Ham Call", "Card Type", "Status", "Missing Training", "Mission Ready", string.Format("Q{0} Missions", quarter + 1), string.Format("Q{0} Meetings", quarter + 1) };
      for (int i = 0; i < headers.Length; i++)
      {
        sheet.Cells[2, i+1].Value = headers[i];
        sheet.Cells[2, i + 1].Style.Font.Bold = true;
      }

      int row = 3;
      foreach (var member in members)
      {
        sheet.Cells[row, 1].Value = member.LastName;
        sheet.Cells[row, 2].Value = member.FirstName;
        sheet.Cells[row, 3].Value = member.ContactNumbers.Where(f => f.Type == "hamcall").Select(f => f.Value).FirstOrDefault();
        sheet.Cells[row, 4].Value = member.WacLevel.ToString();
        sheet.Cells[row, 5].Value = member.Memberships.Where(f => f.Unit.Id == id && f.EndTime == null).Select(f => f.Status.StatusName).FirstOrDefault();


        var expires = CompositeTrainingStatus.Compute(member, courses, today);

        List<string> missingCourses = new List<string>();
        foreach (var course in courses)
        {
          if (!expires.Expirations[course.Id].Completed.HasValue) missingCourses.Add(course.DisplayName);
        }

        sheet.Cells[row, 6].Value = string.Join(", ", missingCourses);



        sheet.Cells[row, 8].Value = member.MissionRosters.Where(f => f.Unit.Id == id && f.TimeIn >= quarterStart && f.TimeIn < quarterStop).Select(f => f.Mission.Id).Distinct().Count();
        var trainingRosters = member.TrainingRosters.Where(f => f.TimeIn >= quarterStart && f.TimeIn < quarterStop).ToList();
        sheet.Cells[row, 9].Value = trainingRosters.Where(f => Regex.IsMatch(f.Training.Title, "IST .*Meeting.*", RegexOptions.IgnoreCase)).Select(f => f.Training.Id).Distinct().Count();

        row++;
      }


      sheet.Cells["A:I"].AutoFitColumns();
    }
  }
}
