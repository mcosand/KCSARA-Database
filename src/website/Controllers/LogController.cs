/*
 * Copyright 2009-2015 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Web.Mvc;
  using Kcsar.Database.Data;

  public class LogController : BaseController
  {
    public LogController(IKcsarContext db) : base(db) { }

    [Authorize(Roles = "cdb.admins")]
    public ViewResult Index()
    {
      return View(this.db.GetLog(DateTime.Now.AddDays(-14)));
    }

    public ContentResult SendDailyMail(string to)
    {
      if (!Permissions.IsUserOrLocal(Request))
      {
        Response.StatusCode = 403;
        return new ContentResult { Content = "Access Denied" };
      }

      if (string.IsNullOrEmpty(to))
      {
        return new ContentResult { Content = "No recipients specified" };
      }

      DateTime start = DateTime.Now.AddHours(-25);


      IList<AuditLogRow> rows = this.db.GetLog(start);
      ViewResult table = View("LogTable", rows);


      if (rows.Count > 0)
      {
        this.MailView(table, to, "KCSARA database changes");
      }

      return new ContentResult { Content = "OK" };

    }
  }
}
