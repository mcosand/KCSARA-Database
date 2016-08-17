using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Kcsara.Database.Model.Training;
using Kcsara.Database.Services.Training;

namespace Kcsara.Database.Api.Controllers
{
  [AnyHostCorsPolicy]
  public class TrainingRecordsController : ApiController
  {
    private readonly ITrainingRecordsService _records;

    public TrainingRecordsController(ITrainingRecordsService records)
    {
      _records = records;
    }

    [HttpGet]
    [Route("members/{memberId}/requiredtraining")]
    public async Task<List<TrainingStatus>> MemberRequired(Guid memberId)
    {
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
