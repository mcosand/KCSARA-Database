using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model.Training;
using Sar.Database.Services;
using Sar.Database.Services.Training;

namespace Kcsara.Database.Api.Controllers
{
  [AnyHostCorsPolicy]
  public class TrainingRecordsController : ApiController
  {
    private readonly ITrainingRecordsService _records;
    private readonly IAuthorizationService _authz;

    public TrainingRecordsController(ITrainingRecordsService records, IAuthorizationService authz)
    {
      _records = records;
      _authz = authz;
    }

    [HttpPost]
    [ValidateModelState]
    [Route("trainingrecords")]
    public async Task<TrainingRecord> CreateNew([FromBody]TrainingRecord record)
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, record.Member.Id, "Create:TrainingRecord@MemberId");
      if (record.Member.Id == Guid.Empty)
      {
        ModelState.AddModelError("Member.Id", "required");
      }
      if (record.Course.Id == Guid.Empty)
      {
        ModelState.AddModelError("Course.Id", "required");
      }

      record = await _records.SaveAsync(record);
      return record;
    }

    [HttpGet]
    [Route("members/{memberId}/trainingrecords")]
    public async Task<List<TrainingStatus>> MemberRecords(Guid memberId)
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, memberId, "Read:TrainingRecord@MemberId");
      return await _records.RecordsForMember(memberId, DateTime.Now);
    }

    [HttpGet]
    [Route("members/{memberId}/requiredtraining")]
    public async Task<List<TrainingStatus>> MemberRequired(Guid memberId)
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, memberId, "Read:TrainingRecord@MemberId");
      return await _records.RequiredTrainingStatusForMember(memberId, DateTime.Now);
    }

    [HttpGet]
    [Route("TrainingRecords/RequiredTraining")]
    public async Task<Dictionary<Guid, List<TrainingStatus>>> Required()
    {
      return await _records.RequiredTrainingStatusForUnit(null, DateTime.Now);
    }

    [HttpPost]
    [Route("TrainingRecords/ParseKcsaraCsv")]
    public async Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv()
    {
      await _authz.EnsureAsync(User as ClaimsPrincipal, null, "Read:TrainingRecord");

      var content = await Request.Content.ReadAsMultipartAsync();

      var file = content.Contents
                  .Where(g => g.Headers.ContentDisposition.Name.Trim('"') == "file")
                  .Cast<StreamContent>()
                  .SingleOrDefault();
      if (file == null) throw new InvalidOperationException();

      var result = await _records.ParseKcsaraCsv(await file.ReadAsStreamAsync());
      return result;
    }
  }
}
