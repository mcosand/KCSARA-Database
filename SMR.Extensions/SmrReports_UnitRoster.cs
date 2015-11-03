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
    private static void UnitRoster(SmrReports me, ExcelPackage package, NameValueCollection queries)
    {
      var sheet = package.Workbook.Worksheets[1];

      var memberships = me.db.Value.Units
        .Where(f => f.DisplayName == "SMR")
        .SelectMany(f => f.Memberships.Where(g => (g.EndTime == null || g.EndTime > DateTime.Now) && g.Status.IsActive));
      var members = memberships
        .Select(f => f.Person);


      var status = memberships.ToDictionary(f => f.Person.Id, f => new { f.Status.StatusName, f.Activated });
      var contacts = members.SelectMany(f => f.ContactNumbers).GroupBy(f => f.Person.Id).ToDictionary(f => f.Key, f => f.ToArray());
      var addresses = members.SelectMany(f => f.Addresses).GroupBy(f => f.Person.Id).ToDictionary(f => f.Key, f => f.ToArray());

      var roster = members.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ThenBy(f => f.Id).ToArray();

      var oldDate = new DateTime(1930, 1, 1);

      for (int i = 0; i < roster.Length; i++)
      {
        var memberId = roster[i].Id;
        var col = 1;
        var row = i + 4;
        sheet.Cells[row, col++].Value = roster[i].LastName;
        sheet.Cells[row, col++].Value = roster[i].FirstName;
        sheet.Cells[row, col++].Value = roster[i].DEM;
        sheet.Cells[row, col++].Value = roster[i].WacLevel == WacLevel.None ? null : roster[i].WacLevel.ToString();
        sheet.Cells[row, col++].Value = status[memberId].StatusName;
        sheet.Cells[row, col++].Value = contacts.ContainsKey(memberId) ? contacts[memberId].Where(f => f.Type == "phone" && f.Subtype == "cell").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault() : null;
        sheet.Cells[row, col++].Value = contacts.ContainsKey(memberId) ? contacts[memberId].Where(f => f.Type == "phone" && f.Subtype == "home").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault() : null;
        sheet.Cells[row, col++].Value = contacts.ContainsKey(memberId) ? contacts[memberId].Where(f => f.Type == "phone" && f.Subtype == "work").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault() : null;
        sheet.Cells[row, col++].Value = contacts.ContainsKey(memberId) ? contacts[memberId].Where(f => f.Type == "email").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault() : null;
        sheet.Cells[row, col++].Value = contacts.ContainsKey(memberId) ? contacts[memberId].Where(f => f.Type == "hamcall").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault() : null;

        var address = addresses.ContainsKey(memberId) ? addresses[memberId].OrderBy(f => f.Type).FirstOrDefault() : null;
        if (address != null)
        {
          sheet.Cells[row, col++].Value = address.Street;
          sheet.Cells[row, col++].Value = address.City;
          sheet.Cells[row, col++].Value = address.State;
          sheet.Cells[row, col++].Value = address.Zip;
        }
        else
        {
          col += 4;
        }

        sheet.Cells[row, col++].Value = status[memberId].Activated > oldDate ? status[memberId].Activated : (DateTime?)null;
      }
      sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }
  }
}
