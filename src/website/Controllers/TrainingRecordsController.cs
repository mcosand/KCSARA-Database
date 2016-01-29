/*
 * Copyright 2013-2016 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.SqlServer;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using Kcsar.Database.Model;
  using Kcsar.Database.Model.Events;
  using log4net;
  using Microsoft.AspNet.Authorization;
  using Microsoft.AspNet.Mvc;
  using Models;
  using Services;
  using Model = Kcsar.Database.Model;
  public class TrainingRecordsController : BaseController
  {
    private readonly ITrainingService service;

    public TrainingRecordsController(Lazy<ITrainingService> service, Lazy<IKcsarContext> db, ILog log) : base(db, log)
    {
      this.service = service.Value;
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/event/{eventId}")]
    public object ApiRecordsForRoster(Guid memberId, Guid eventId)
    {
      return service.ListRecords(row => row.Event != null && row.Event.Id == eventId && row.Member.Id == memberId);
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/latest")]
    public object ApiRecordsForMember(Guid memberId)
    {
      return service.ListRecords(row => row.Member.Id == memberId, true);
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/required")]
    public async Task<object> ApiRequiredForMember(Guid memberId)
    {
      return await service.ListRequired(memberId);
    }
  }
}