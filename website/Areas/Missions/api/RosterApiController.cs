/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Areas.Missions.api
{
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
  using Kcsara.Database.Web.Model;

  [ModelValidationFilter]
  public class RosterApiController : BaseApiController
  {
    public static readonly string RouteName = "api_MissionsRoster";

    public RosterApiController(Kcsara.Database.Web.Controllers.ControllerArgs args)
      : base(args)
    { }

    [HttpGet]
    [Authorize]
    public RespondingUnit[] GetRespondingUnits(Guid id, DateTime? time)
    {
      return this.db.Missions.Where(f => f.Id == id)
        .SelectMany(f => f.RespondingUnits)
        .Select(f => new RespondingUnit
        {
        }).ToArray();
    }

    [HttpGet]
    [Authorize]
    public MissionResponse[] GetMemberResponses(Guid id)
    {
      return this.db.Missions
        .SelectMany(f => f.Responders)
        .Where(f => f.MemberId == id)
        .OrderByDescending(f => f.Mission.StartTime)
        .AsEnumerable()
        .Select(f => MissionResponse.FromDatabase(f, doResponder: false))
        .ToArray();
    }
  }
}