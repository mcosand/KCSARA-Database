/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.AspNet.Hosting;
  using Models;
  using OfficeOpenXml;
  /// <summary>
  /// 
  /// </summary>
  public interface IUnitsService
  {
    IEnumerable<NameIdPair> List();
    SarUnit GetUnit(Guid id);
    object GetRoster(Guid unitId);
    DownloadStream GetRosterReport(Guid? unitId);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="RowType"></typeparam>
  /// <typeparam name="ModelType"></typeparam>
  public class UnitsService : IUnitsService
  {
    private readonly Func<IKcsarContext> dbFactory;
    private readonly ILog log;
    private readonly IHostingEnvironment hosting;

    public UnitsService(Func<IKcsarContext> dbFactory, IHostingEnvironment hosting, ILog log)
    {
      this.dbFactory = dbFactory;
      this.hosting = hosting;
      this.log = log;
    }

    public object GetRoster(Guid unitId)
    {
      using (var db = dbFactory())
      {
        var members = db.UnitMemberships.Where(um => um.Unit.Id == unitId && um.EndTime == null);
        members = members.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

        return members.Select(f => new
        {
          Member = new MemberSummary { Id = f.Person.Id, Name = f.Person.LastName + ", " + f.Person.FirstName, Photo = f.Person.PhotoFile, WorkerNumber = f.Person.DEM },
          Status = f.Status.StatusName,
          IsActive = f.Status.IsActive,
          AsOf = f.Activated
        }).ToList();
      }
    }

    public SarUnit GetUnit(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.Units.Select(f => new SarUnit { Id = f.Id, Name = f.DisplayName }).SingleOrDefault(f => f.Id == id);
      }
    }

    public IEnumerable<NameIdPair> List()
    {
      using (var db = dbFactory())
      {
        return db.Units
          .Select(f => new NameIdPair { Id = f.Id, Name = f.DisplayName })
          .OrderBy(f => f.Name)
          .ToList();
      }
    }

    public DownloadStream GetRosterReport(Guid? id)
    {
      // The method almost supports id=null as downloading the KCSARA roster
      // It doesn't do a distinct(person), so people in multiple units are recorded more than once.
      ExcelPackage xl;
      using (FileStream fs = new FileStream(hosting.MapPath("templates/roster-template.xlsx"), FileMode.Open, FileAccess.Read))
      {
        xl = new ExcelPackage(fs);
      }

      var ws = xl.Workbook.Worksheets[1];
      string filename = string.Format("roster-{0:yyMMdd}.xlsx", DateTime.Now);

      using (var db = dbFactory())
      {
        var memberships = db.UnitMemberships.Include("Person").Include("Person.Addresses").Include("Person.ContactNumbers").Include("Status");
        string unitShort = string.Empty; //ConfigurationManager.AppSettings["dbNameShort"];
        string unitLong = Strings.GroupName;
        if (id.HasValue)
        {
          memberships = memberships.Where(um => um.Unit.Id == id.Value);
          SarUnitRow sarUnit = (from u in db.Units where u.Id == id.Value select u).First();
          unitShort = sarUnit.DisplayName;
          unitLong = sarUnit.LongName;

        }
        memberships = memberships.Where(um => um.EndTime == null && um.Status.IsActive);
        memberships = memberships.OrderBy(f => f.Person.LastName).ThenBy(f => f.Person.FirstName);

        ws.HeaderFooter.FirstHeader.CenteredText = unitLong + " Active Roster";
        ws.HeaderFooter.FirstFooter.CenteredText = DateTime.Now.ToShortDateString();
        filename = unitShort + "-" + filename;

        int idx = 2;
        int c = 1;
        foreach (UnitMembership membership in memberships)
        {
          MemberRow member = membership.Person;
          c = 1;
          ws.Cells[idx, c++].Value = string.Format("{0:0000}", member.DEM);
          ws.Cells[idx, c++].Value = member.LastName;
          ws.Cells[idx, c++].Value = member.FirstName;
          ws.Cells[idx, c++].Value = string.Join("\n", member.Addresses.Select(f => f.Street).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.Addresses.Select(f => f.City).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.Addresses.Select(f => f.State).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.Addresses.Select(f => f.Zip).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.ContactNumbers.Where(f => f.Type == "phone" && f.Subtype == "home").Select(f => f.Value).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.ContactNumbers.Where(f => f.Type == "phone" && f.Subtype == "cell").Select(f => f.Value).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.ContactNumbers.Where(f => f.Type == "email").Select(f => f.Value).ToArray());
          ws.Cells[idx, c++].Value = string.Join("\n", member.ContactNumbers.Where(f => f.Type == "hamcall").Select(f => f.Value).ToArray());
          ws.Cells[idx, c++].Value = member.WacLevel.ToString();
          ws.Cells[idx, c++].Value = membership.Status.StatusName;

          /*  if ((includeHidden ?? false) && (Permissions.IsAdmin || (id.HasValue && Permissions.IsMembershipForUnit(id.Value))))
            {
              wrap.SetCellValue("DOB", 0, c);
              wrap.SetCellValue(string.Format("{0:yyyy-MM-dd}", member.BirthDate);
              wrap.SetCellValue("Gender", 0, c);
              wrap.SetCellValue(member.Gender.ToString();
            }
            */
          idx++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
      }

      MemoryStream ms = new MemoryStream();
      xl.SaveAs(ms);
      ms.Seek(0, SeekOrigin.Begin);
      return new DownloadStream { Stream = ms, MimeType = "application/vnd.ms-excel", Filename = filename };
    }
  }
}
