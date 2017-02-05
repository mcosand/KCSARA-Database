using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Model;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers
{
  [Route("trainings")]
  public class TrainingsController : EventsController
  {
    public TrainingsController(ITrainingsService trainings, IAuthorizationService authz) : base(trainings, authz)
    {
    }
  }
}
