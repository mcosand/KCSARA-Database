/*
 * Copyright 2013-2016 Matthew Cosand
 */

namespace Kcsara.Database.Web.Controllers
{
  using System;
  using System.Threading.Tasks;
  using Kcsar.Database.Model;
  using Microsoft.AspNet.Mvc;
  using Microsoft.Extensions.Logging;
  using Services;
  public class TrainingRecordsController : BaseController
  {
    private readonly ITrainingService service;

    public TrainingRecordsController(Lazy<ITrainingService> service, Lazy<IKcsarContext> db, ILogger<TrainingRecordsController> log) : base(db, log)
    {
      this.service = service.Value;
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/event/{eventId}")]
    public async Task<object> ApiRecordsForRoster(Guid memberId, Guid eventId)
    {
      return await service.ListRecords(row => row.Event != null && row.Event.Id == eventId && row.Member.Id == memberId);
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/latest")]
    public async Task<object> ApiRecordsForMember(Guid memberId)
    {
      return await service.ListRecords(row => row.Member.Id == memberId, true);
    }

    [HttpGet]
    [Route("api/members/{memberId}/training/required")]
    public async Task<object> ApiRequiredForMember(Guid memberId)
    {
      return await service.ListRequired(memberId);
    }
  }
}