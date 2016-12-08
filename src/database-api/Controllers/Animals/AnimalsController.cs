using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Animals;
using Sar.Database.Model.Training;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Animals
{
  public class AnimalsController : ApiController
  {
    private readonly IAnimalsService _animals;
    private readonly IAuthorizationService _authz;

    public AnimalsController(IAnimalsService animals, IAuthorizationService authz)
    {
      _animals = animals;
      _authz = authz;
    }
    /*
    [HttpGet]
    [Route("training/courses/{courseId}")]
    public async Task<ItemPermissionWrapper<TrainingCourse>> GetCourse(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _animals.GetAsync(courseId);
    }

    [HttpGet]
    [Route("training/courses/{courseId}/stats")]
    public async Task<object> GetCourseStats(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _animals.GetCourseStats(courseId);
    }

    [HttpGet]
    [Route("training/courses/{courseId}/roster")]
    public async Task<List<TrainingRecord>> ListCourseRoster(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      await _authz.EnsureAsync(null, "Read:Member");

      return await _animals.ListRoster(courseId);
    }
    */
    [HttpGet]
    [Route("animals")]
    public async Task<ListPermissionWrapper<Animal>> List()
    {
      await _authz.EnsureAsync(null, "Read:Animal");
      return await _animals.List();
    }
    /*
    [HttpPost]
    [ValidateModelState]
    [Route("training/courses")]
    public async Task<TrainingCourse> CreateNew([FromBody]TrainingCourse course)
    {
      await _authz.EnsureAsync(null, "Create:TrainingCourse");

      if (course.Id != Guid.Empty)
      {
        throw new UserErrorException("New units shouldn't include an id");
      }

      course = await _animals.SaveAsync(course);
      return course;
    }

    [HttpPut]
    [ValidateModelState]
    [Route("training/courses/{courseId}")]
    public async Task<TrainingCourse> Save(Guid courseId, [FromBody]TrainingCourse course)
    {
      await _authz.EnsureAsync(courseId, "Update:TrainingCourse");

      if (course.Id != courseId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      course = await _animals.SaveAsync(course);
      return course;
    }



    [HttpDelete]
    [Route("training/courses/{courseId}")]
    public async Task Delete(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Delete:Unit");

      await _animals.DeleteAsync(courseId);
    }
    */
  }
}
