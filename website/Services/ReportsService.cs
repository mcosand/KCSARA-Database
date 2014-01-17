/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Web;
  using Kcsar.Database;
  using Kcsar.Database.Model;
  using Kcsara.Database.Web;

  /// <summary>
  /// 
  /// </summary>
  public class ReportsService : IReportsService
  {
    private readonly IKcsarContext db;
    private readonly IAppSettings settings;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="settings"></param>
    public ReportsService(IKcsarContext db, IAppSettings settings)
    {
      this.db = db;
      this.settings = settings;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public Stream GetMissionReadyList(SarUnit unit)
    {
      string templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Report", "missionready-template.xls");

      ExcelFile xl;
      using (FileStream fs = new FileStream(templateFile, FileMode.Open, FileAccess.Read))
      {
        xl = ExcelService.Read(fs, ExcelFileType.XLS);
      }

      var goodList = xl.GetSheet(0);

      GenerateMissionReadySheets(unit, xl, goodList);

      MemoryStream ms = new MemoryStream();
      xl.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return ms;
    }

    public Stream GetMembershipReport()
    {
      ExcelFile xl = ExcelService.Create(ExcelFileType.XLS);
      var goodList = xl.CreateSheet("Mission Ready");

      this.GenerateMissionReadySheets(null, xl, goodList);

      var dataSheet = xl.CreateSheet("Member Data");

      var interestingCourses = (from c in this.db.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToList();
      interestingCourses.AddRange(this.db.GetCoreCompetencyCourses());


      IQueryable<Member> members = this.db.Members.Include("Addresses", "Memberships", "ComputedAwards.Course").Where(f => f.Memberships.Any(g => g.Status.IsActive && g.EndTime == null));
      members = members.OrderBy(f => f.LastName).ThenBy(f => f.FirstName);

      // Set column header titles. A static list, followed by a list of "interesting" training courses
      var columns = new[] { "DEM", "Lastname", "Firstname", "WAC Card", "Street", "City", "State", "ZIP", "Phone", "Email", "HAM", "Units" }.Union(interestingCourses.Select(f => f.DisplayName)).ToArray();
      for (int i = 0; i < columns.Length; i++)
      {
        dataSheet.CellAt(0, i).SetValue(columns[i]);
        dataSheet.CellAt(0, i).SetBold(true);
      }

      int row = 1;
      foreach (var m in members)
      {
        int col = 0;
        dataSheet.CellAt(row, col++).SetValue(m.DEM);
        dataSheet.CellAt(row, col++).SetValue(m.LastName);
        dataSheet.CellAt(row, col++).SetValue(m.FirstName);
        dataSheet.CellAt(row, col++).SetValue(m.WacLevel.ToString());

        var address = m.Addresses.OrderBy(f => f.InternalType).FirstOrDefault();
        if (address != null)
        {
          dataSheet.CellAt(row, col++).SetValue(address.Street);
          dataSheet.CellAt(row, col++).SetValue(address.City);
          dataSheet.CellAt(row, col++).SetValue(address.State);
          dataSheet.CellAt(row, col++).SetValue(address.Zip);
        }
        else
        {
          col += 4;
        }

        Action<Member, string> doContact = (member, type) =>
        {
          var phone = m.ContactNumbers.Where(f => f.Type == type).OrderBy(f => f.Priority).FirstOrDefault();
          if (phone != null)
          {
            dataSheet.CellAt(row, col).SetValue(phone.Value);
          }
          col++;
        };
        doContact(m, "phone");
        doContact(m, "email");
        doContact(m, "hamcall");

        dataSheet.CellAt(row, col++).SetValue(string.Join(" ",
            m.Memberships.Where(f => f.Status.IsActive && f.EndTime == null).Select(f => f.Unit.DisplayName).OrderBy(f => f)
          ));

        var trainingStatus = CompositeTrainingStatus.Compute(m, interestingCourses, DateTime.Now);
        for (int i = 0; i < interestingCourses.Count; i++)
        {
          dataSheet.CellAt(row, col++).SetValue(trainingStatus.Expirations[interestingCourses[i].Id].ToString());
        }
        row++;
      }
      //IQueryable<UnitMembership> memberships = this.db.UnitMemberships.Include("Person.Addresses", "Person.ContactNumbers").Include("Status");
      //memberships = memberships.Where(um => um.EndTime == null && um.Status.IsActive);
      //memberships = memberships.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);



      MemoryStream ms = new MemoryStream();
      xl.Save(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return ms;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="xl"></param>
    /// <param name="goodList"></param>
    public void GenerateMissionReadySheets(SarUnit unit, ExcelFile xl, ExcelSheet goodList)
    {
      string longName = this.settings.GroupFullName ?? this.settings.GroupName;
      IQueryable<UnitMembership> memberships = this.db.UnitMemberships.Include("Person.ComputedAwards.Course").Include("Status");
      if (unit != null)
      {
        memberships = memberships.Where(um => um.Unit.Id == unit.Id);
        longName = unit.LongName;

      }
      memberships = memberships.Where(um => um.EndTime == null && um.Status.IsActive);
      memberships = memberships.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

      goodList.Header = longName + " Mission Active Roster";
      goodList.Footer = DateTime.Now.ToShortDateString();

      var courses = (from c in this.db.TrainingCourses where c.WacRequired > 0 select c).OrderBy(x => x.DisplayName).ToList();

      int headerIdx = 4;
      foreach (var course in courses)
      {
        var cell = goodList.CellAt(0, headerIdx++);
        cell.SetValue(course.DisplayName);

        cell.SetBold(true);
        cell.SetTextWrap(true);
        //cell.Style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
        //cell.Style.VerticalAlignment = VerticalAlignmentStyle.Bottom;
      }

      ExcelSheet badList = xl.CopySheet(goodList.Name, "Non-Mission Members");
      ExcelSheet nonFieldList = xl.CopySheet(goodList.Name, "Admin Members");

      using (SheetAutoFitWrapper good = new SheetAutoFitWrapper(xl, goodList))
      {
        using (SheetAutoFitWrapper bad = new SheetAutoFitWrapper(xl, badList))
        {
          using (SheetAutoFitWrapper admin = new SheetAutoFitWrapper(xl, nonFieldList))
          {
            int idx = 1;
            int c = 0;
            Guid lastId = Guid.Empty;

            foreach (UnitMembership membership in memberships)
            {
              Member member = membership.Person;
              if (member.Id == lastId)
              {
                continue;
              }
              lastId = member.Id;

              CompositeTrainingStatus stats = CompositeTrainingStatus.Compute(member, courses, DateTime.Now);

              SheetAutoFitWrapper wrap = bad;
              // If the person isn't supposed to keep up a WAC card, then they're administrative...
              if (membership.Status.WacLevel == WacLevel.None)
              {
                wrap = admin;
              }
              // If they're current on training and have a DEM card, they're good...
              else if (stats.IsGood && member.WacLevel != WacLevel.None)
              {
                wrap = good;
              }
              idx = wrap.Sheet.NumRows + 1;
              c = 0;

              wrap.SetCellValue(string.Format("{0:0000}", member.DEM), idx, c++);
              wrap.SetCellValue(member.LastName, idx, c++);
              wrap.SetCellValue(member.FirstName, idx, c++);
              ExcelCell cell = wrap.Sheet.CellAt(idx, c);
              switch (member.WacLevel)
              {
                case WacLevel.Field:
                  cell.SetFillColor(Color.Green);
                  cell.SetFontColor(Color.White);
                  break;
                case WacLevel.Novice:
                  cell.SetFillColor(Color.Red);
                  cell.SetFontColor(Color.White);
                  break;
                case WacLevel.Support:
                  cell.SetFillColor(Color.Orange);
                  break;
              }
              wrap.SetCellValue(member.WacLevel.ToString(), idx, c++);
              foreach (var course in courses)
              {
                TrainingStatus stat = stats.Expirations[course.Id];

                if ((stat.Status & ExpirationFlags.Okay) != ExpirationFlags.Okay)
                {
                  wrap.Sheet.CellAt(idx, c).SetFillColor(Color.Pink);
                  wrap.Sheet.CellAt(idx, c).SetBorderColor(Color.Red);
                }

                wrap.SetCellValue(stat.ToString(), idx, c);
                if (stat.Expires.HasValue)
                {
                  wrap.Sheet.CellAt(idx, c).SetValue(stat.Expires.Value.Date.ToString("yyyy-MM-dd"));
                }

                c++;
              }
              if (wrap == bad)
              {
                wrap.Sheet.CellAt(idx, c).SetValue(member.ContactNumbers.Where(f => f.Type == "email").OrderBy(f => f.Priority).Select(f => f.Value).FirstOrDefault());
              }
              idx++;
            }
            admin.Sheet.AutoFitAll();
            good.Sheet.AutoFitAll();
            bad.Sheet.AutoFitAll();
          }
        }
      }
    }
  }
}
