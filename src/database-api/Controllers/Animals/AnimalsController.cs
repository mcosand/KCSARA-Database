using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    private readonly IHost _host;

    public AnimalsController(IAnimalsService animals, IAuthorizationService authz, IHost host)
    {
      _animals = animals;
      _authz = authz;
      _host = host;
    }
    
    [HttpGet]
    [Route("animals/{animalId}")]
    public async Task<ItemPermissionWrapper<Animal>> GetAnimal(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      return await _animals.GetAsync(animalId);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("animals/{animalId}/photo")]
    public async Task<HttpResponseMessage> Photo(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      var animal = (await _animals.GetAsync(animalId))?.Item;

      string filename = "content\\images\\nophoto.jpg";
      if (!string.IsNullOrWhiteSpace(animal?.Photo) && _host.FileExists("content\\auth\\animals\\" + animal.Photo))
      {
        filename = "content\\auth\\animals\\" + animal.Photo;
      }

      Stream imageStream = _host.OpenFile(filename);
      var response = new HttpResponseMessage
      {
        Content = new StreamContent(imageStream)
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");

      return response;
    }

    [HttpGet]
    [Route("animals/{animalId}/owners")]
    public async Task<ListPermissionWrapper<AnimalOwner>> ListOwners(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");

      return await _animals.ListOwners(animalId);
    }

    [HttpPut]
    [ValidateModelState]
    [Route("animals/{animalId}/owners/{ownershipId}")]
    public async Task<AnimalOwner> SaveOwner(Guid animalId, Guid ownershipId, [FromBody]AnimalOwner ownership)
    {
      await _authz.EnsureAsync(ownershipId, "Update:AnimalOwner");

      if (ownership.Animal.Id != animalId) ModelState.AddModelError("animal.id", "Can not be changed");
      if (ownership.Id != ownershipId) ModelState.AddModelError("id", "Can not be changed");

      if (!ModelState.IsValid) throw new UserErrorException("Invalid parameters");

      ownership = await _animals.SaveOwnership(ownership);
      return ownership;
    }

    [HttpPost]
    [ValidateModelState]
    [Route("animals/{animalId}/owners")]
    public async Task<AnimalOwner> CreateNewOwner(Guid animalId, [FromBody]AnimalOwner ownership)
    {
      await _authz.EnsureAsync(animalId, "Create:AnimalOwner@AnimalId");
      await _authz.EnsureAsync(ownership?.Member?.Id, "Create:AnimalOwner@MemberId");

      if (ownership.Id != Guid.Empty)
      {
        throw new UserErrorException("New animal ownership shouldn't include an id");
      }

      if (ownership.Animal != null && ownership.Animal.Id != animalId)
      {
        throw new UserErrorException("Animal ids do not match", string.Format("Tried to save animal owner with animal id {0} under animal {1}", ownership.Animal.Id, animalId));
      }

      ownership = await _animals.SaveOwnership(ownership);
      return ownership;
    }

    [HttpDelete]
    [Route("animals/{animalId}/owners/{ownershipId}")]
    public async Task DeleteOwner(Guid animalId, Guid ownershipId)
    {
      await _authz.EnsureAsync(animalId, "Delete:AnimalOwner");

      await _animals.DeleteOwnership(ownershipId);
    }

    [HttpGet]
    [Route("animals/{animalId}/missions/stats")]
    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");

      return await _animals.GetMissionStatistics(animalId);
    }

    [HttpGet]
    [Route("animals/{animalId}/missions")]
    public async Task<List<EventAttendance>> ListMissions(Guid animalId)
    {
      await _authz.EnsureAsync(animalId, "Read:Animal");
      await _authz.EnsureAsync(null, "Read:Mission");

      return await _animals.GetMissionList(animalId);
    }

    [HttpGet]
    [Route("animals")]
    public async Task<ListPermissionWrapper<Animal>> List()
    {
      await _authz.EnsureAsync(null, "Read:Animal");
      return await _animals.List();
    }

  }
}
