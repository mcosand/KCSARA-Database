using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Services.Training;

namespace Kcsara.Database.Api.Controllers
{
  public class TrainingRecordsController : ApiController
  {
    private readonly ITrainingRecordsService _records;

    public TrainingRecordsController(ITrainingRecordsService records)
    {
      _records = records;
    }

    [HttpGet]
    [Route("members/{memberId}/training")]
    public async Task<List<Model.Training.CompletedTrainingStatus>> MemberTraining(Guid memberId)
    {
      return await _records.TrainingForMember(memberId, DateTime.Now, true);
    }

    [HttpGet]
    [Route("members/{memberId}/pasttraining")]
    public async Task<List<Model.Training.CompletedTrainingStatus>> PastMemberTraining(Guid memberId)
    {
      return await _records.TrainingForMember(memberId, DateTime.Now, false);
    }

    [HttpGet]
    [Route("members/{memberId}/requiredtraining")]
    public async Task<List<Model.Training.TrainingStatus>> MemberRequired(Guid memberId)
    {
      return await _records.RequiredTrainingStatusForMember(memberId, DateTime.Now);
    }

    [HttpGet]
    [Route("RequiredTraining")]
    public async Task<Dictionary<Guid, List<Model.Training.TrainingStatus>>> Required()
    {
      return await _records.RequiredTrainingStatusForUnit(null, DateTime.Now);
    }
  }
}
