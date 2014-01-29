/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Areas.Missions.api
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Text;
  using System.Web.Http;
  using Kcsara.Database.Web.api;
  using Kcsara.Database.Web.api.Models;
  using M = Kcsar.Database.Model;

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
        .AsEnumerable()
        .Select(f => MissionResponseStatus.FromData(f))
        .ToArray();

      return result;
    }

    [HttpGet]
    [Authorize]
    public MissionResponseInfo GetMissionInfo(Guid id)
    {
      var mission = this.GetObjectOrNotFound(() => this.db.Missions.SingleOrDefault(f => f.Id == id));

      return MissionResponseInfo.FromData(mission);
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

      var result = MissionResponseStatus.FromData(m);

      return result;
    }
  }
}