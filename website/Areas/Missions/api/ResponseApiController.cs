/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Areas.Missions.api
{
  using System;
  using System.Data.Entity.Spatial;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Text;
  using System.Web.Http;
  using Kcsara.Database.Web.api;
  using Kcsara.Database.Web.api.Models;
  using Kcsara.Database.Web.Missions.Hubs;
  using M = Kcsar.Database.Model;

  [ModelValidationFilter]
  public class ResponseApiController : BaseApiController
  {
    public static readonly string RouteName = "api_MissionsResponse";
    protected readonly ResponseHubClient responseHub;

    public ResponseApiController(ResponseHubClient responseHub, Kcsara.Database.Web.Controllers.ControllerArgs args)
      : base(args)
    {
      this.responseHub = responseHub;
    }

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

    [HttpPost]
    [Authorize]
    public bool Checkin([FromUri] Guid id, [FromBody] ResponderCheckin checkin)
    {
      M.Mission mission = this.db.Missions.SingleOrDefault(f => f.Id == id);
      if (mission == null) throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Mission not found") });

      M.MissionRespondingUnit respondingUnit = mission.RespondingUnits.SingleOrDefault(f => f.Id == checkin.Unit.Id);
      if (respondingUnit == null)
      {
        respondingUnit = new M.MissionRespondingUnit
        {
          IsActive = true,
          Mission = mission,
          UnitId = checkin.Unit.UnitId
        };
        mission.RespondingUnits.Add(respondingUnit);
      }

      Guid memberId = this.Permissions.UserId;

      M.MissionResponder missionResponder = mission.Responders
        .Where(f => f.MemberId == memberId)
        .OrderByDescending(f => f.LastTimeline == null ? DateTime.MinValue : f.LastTimeline.Time)
        .FirstOrDefault();

      if (missionResponder == null)
      {
        M.Member member = this.db.Members.SingleOrDefault(f => f.Id == memberId);

        missionResponder = new M.MissionResponder
        {
          Member = member,
          Mission = mission,
          Role = checkin.Role.ToString(),
          RespondingUnit = respondingUnit
        };
        mission.Responders.Add(missionResponder);
      }
      this.db.SaveChanges();

      M.MissionResponderTimelime timeline = new M.MissionResponderTimelime
      {
        Responder = missionResponder,
        Status = checkin.Status,
        Time = DateTime.Now,
      };

      if (checkin.Location != null && checkin.Location.Type == "geo")
      {
        timeline.Location = DbGeography.PointFromText(string.Format("POINT({0} {1})", checkin.Location.Coords.longitude, checkin.Location.Coords.latitude), DbGeography.DefaultCoordinateSystemId);
      }

      missionResponder.Timeline.Add(timeline);
      missionResponder.LastTimeline = missionResponder.Timeline.OrderByDescending(f => f.Time).First();
      
      this.db.SaveChanges();
      this.responseHub.BroadcastRespondersUpdate(id);
      return true;
    }

    public string testhub(string msg)
    {
      this.responseHub.BroadcastRespondersUpdate(Guid.NewGuid());
      return msg;
    }

    public MissionResponse[] GetResponders(Guid id)
    {
      var result = this.db.Missions.Where(f => f.Id == id).SelectMany(f => f.Responders)
        .Where(f => f.Hours == null)
        .AsEnumerable()
        .Select(f => MissionResponse.FromDatabase(f, doResponder: true, doUnit: true))
        .ToArray();

      return result;
    }
  }
}