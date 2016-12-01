using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar;
using Sar.Database.Model;
using Sar.Database.Model.Training;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Training
{
  public class TrainingCoursesController : ApiController
  {
    private readonly ITrainingCoursesService _courses;
    private readonly IAuthorizationService _authz;

    public TrainingCoursesController(ITrainingCoursesService courses, IAuthorizationService authz)
    {
      _courses = courses;
      _authz = authz;
    }

    [HttpGet]
    [Route("training/courses/{courseId}")]
    public async Task<ItemPermissionWrapper<TrainingCourse>> GetCourse(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _courses.GetAsync(courseId);
    }

    [HttpGet]
    [Route("training/courses/{courseId}/stats")]
    public async Task<object> GetCourseStats(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      return await _courses.GetCourseStats(courseId);
    }

    [HttpGet]
    [Route("training/courses/{courseId}/roster")]
    public async Task<List<TrainingRecord>> ListCourseRoster(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Read:TrainingCourse");
      await _authz.EnsureAsync(null, "Read:Member");

      return await _courses.ListRoster(courseId);
    }

    [HttpGet]
    [Route("training/courses")]
    public async Task<ListPermissionWrapper<TrainingCourse>> List()
    {
      await _authz.EnsureAsync(null, "Read:TrainingCourse");
      return await _courses.List();
    }

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

      course = await _courses.SaveAsync(course);
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

      course = await _courses.SaveAsync(course);
      return course;
    }



    [HttpDelete]
    [Route("training/courses/{courseId}")]
    public async Task Delete(Guid courseId)
    {
      await _authz.EnsureAsync(courseId, "Delete:Unit");

      await _courses.DeleteAsync(courseId);
    }
  }
}
