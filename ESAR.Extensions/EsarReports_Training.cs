namespace ESAR.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;
  using Kcsar.Database.Model;
  using OfficeOpenXml;

  public partial class EsarReports
  {
    private static void TrainingReport(EsarReports me, ExcelPackage package, NameValueCollection queries)
    {
      DateTime today = DateTime.Today;
      var sheet = package.Workbook.Worksheets[1];
      var db = me.db.Value;
      int row = 1;

      var courseNames = new List<string>(new[] { "Course A", "Course B", "Course C", "NIMS I-100", "NIMS I-700", "Course I", "Course II", "Course III" });
      var courses = (from tc in db.TrainingCourses where (tc.Unit.DisplayName == "ESAR" && tc.Categories.Contains("basic")) || tc.DisplayName.StartsWith("NIMS ") orderby tc.DisplayName select tc).ToArray()
          .Where(f => courseNames.Contains(f.DisplayName)).OrderBy(f => courseNames.IndexOf(f.DisplayName)).ToList();


      foreach (var traineeRow in (from um in db.UnitMemberships.Include("Person.ComputedAwards.Course").Include("Person.ContactNumbers")
                                  where um.Unit.DisplayName == "ESAR" && um.Status.StatusName == "trainee" && um.EndTime == null
                                  orderby um.Person.FirstName
                                  orderby um.Person.LastName
                                  select new { p = um.Person, a = um.Person.ComputedAwards, n = um.Person.ContactNumbers }))
      {
        row++;
        Member trainee = traineeRow.p;

        int col = 1;
        sheet.Cells[row, col++].Value = trainee.LastName;

        sheet.Cells[row, col++].Value = trainee.FirstName;
        sheet.Cells[row, col++].Value = trainee.BirthDate.HasValue ? ((trainee.BirthDate.Value.AddYears(21) > DateTime.Today) ? "Y" : "A") : "?";
        sheet.Cells[row, col++].Value = trainee.Gender.ToString().Substring(0, 1);
        sheet.Cells[row, col++].Value = string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "phone" && f.Subtype.ToLowerInvariant() == "home").Select(f => f.Value).ToArray());
        sheet.Cells[row, col++].Value = string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "phone" && f.Subtype.ToLowerInvariant() == "cell").Select(f => f.Value).ToArray());
        sheet.Cells[row, col++].Value = string.Join("\n", trainee.ContactNumbers.Where(f => f.Type.ToLowerInvariant() == "email").Select(f => f.Value).ToArray());

        int nextCourseCol = col++;
        bool foundNext = false;

        string courseName = string.Format("{0}", sheet.Cells[1, col].Text);
        while (!string.IsNullOrWhiteSpace(courseName))
        {
          var record = trainee.ComputedAwards.FirstOrDefault(f => f.Course.DisplayName == courseName && (f.Expiry == null || f.Expiry > today));

          if (courseName.Equals("Worker App") && trainee.SheriffApp.HasValue)
          {
            sheet.Cells[row, col].Value = "X";
          }
          else if (courseName.Equals("BG Check") && trainee.BackgroundDate.HasValue)
          {
            sheet.Cells[row, col].Value = "X";
          }
          else if (record != null)
          {
            sheet.Cells[row, col].Value = record.Completed.Value.Date;
          }
          else if (foundNext == false)
          {
            sheet.Cells[row, nextCourseCol].Value = courseName;
            foundNext = true;
          }

          col++;
          courseName = string.Format("{0}", sheet.Cells[1, col].Text);
        }

        if (trainee.PhotoFile != null) sheet.Cells[row, col++].Value = "havePhoto";

      }

      sheet.Cells["A:R"].AutoFitColumns();
    }
  }
}
