using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kcsara.Database.Web.api;
using M = Kcsar.Database.Model;
using Kcsara.Database.Web.api.Models;
using Kcsara.Database.Services;
using log4net;
using System.Text;

namespace Kcsara.Database.Web.Areas.Missions.api
{
  [ModelValidationFilter]
  public class ResponseApiController : BaseApiController
  {
    public static readonly string RouteName = "api_MissionsResponse";
    
    public ResponseApiController(Kcsara.Database.Web.Controllers.ControllerArgs args)
      : base(args)
    { }

    [HttpGet]
    [Authorize]
    public MissionResponseStatus[] GetCurrentStatus()
    {
      DateTime window = DateTime.Now.AddDays(-7);
      var result = this.db.Missions
        .Where(f => f.StartTime > window || f.Roster.Any(g => g.TimeIn > window) || (f.ResponseStatus != null && f.ResponseStatus.CallForPeriod > window))
        .OrderBy(f => f.StartTime)
        .Select(MissionResponseStatus.FromDatabase)
        .ToArray();

      return result;
    }

    [HttpPost]
    [Authorize]
    public MissionResponseStatus Create(CreateMission mission)
    {
      if (mission.Started < DateTime.Now.AddHours(-1))
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("{\"mission.Started\":[\"Must be within the past hour\"]}", Encoding.UTF8, "application/json") });
      }

      M.Mission m = new M.Mission
      {
        Title = mission.Title,
        Location = mission.Location,
        StartTime = mission.Started
      };

      M.MissionResponseStatus status = new M.MissionResponseStatus
      {
        CallForPeriod = mission.Started
      };

      m.ResponseStatus = status;

      db.Missions.Add(m);
      db.SaveChanges();

      var result = db.Missions
        .Where(f => f.Id == m.Id)
        .Select(MissionResponseStatus.FromDatabase)
        .Single();

      return result;
    }
  }
}