/*
 * Copyright 2013-2014 Matthew Cosand
 */
using Kcsar.Database.Model;
using Kcsara.Database.Web.Model;
using Kcsara.Database.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using Kcsara.Database.Web.Services;

namespace Kcsara.Database.Web.api
{
  public class MissionsController : BaseApiController
  {
    public MissionsController(IKcsarContext db, IAuthService auth, ILog log)
      : base(db, auth, log)
    { }

    [HttpGet]
    [Authorize(Roles="cdb.users")]
    public IEnumerable<MemberDetailView> GetResponderEmails(Guid id, Guid? unitId)
    {
      string unit = null;

      var q = db.MissionRosters.Where(f => f.Mission.Id == id);
      if (unitId.HasValue)
      {
        q = q.Where(f => f.Unit.Id == unitId.Value);
        unit = db.Units.Single(f => f.Id == unitId).DisplayName;
      }

      var responders = q.Select(f => new
      {
        Id = f.Person.Id,
        First = f.Person.FirstName,
        Last = f.Person.LastName,
        Email = f.Person.ContactNumbers.Where(g => g.Type == "email").OrderBy(g => g.Priority).FirstOrDefault(),
        Unit = f.Unit.DisplayName
      }).Distinct().OrderBy(f => f.Last).ThenBy(f => f.First).ToArray();

      var model = responders.Select(f => new MemberDetailView
      {
        Id = f.Id,
        FirstName = f.First,
        LastName = f.Last,
        Units = new[] { f.Unit },
        Contacts = new[] { f.Email == null ? null : new MemberContactView { Id = f.Email.Id, Value = f.Email.Value } }
      });

      return model;
    }
  }
}
