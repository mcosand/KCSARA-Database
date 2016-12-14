using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Sar.Database.Data;
using Sar.Database.Model;
using Sar.Database.Model.Animals;
using Sar.Database.Model.Members;
using DB = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IAnimalsService
  {
    Task<ListPermissionWrapper<Animal>> List();
    //Task<ListPermissionWrapper<UnitMembership>> ListMemberships(Expression<Func<UnitMembership, bool>> predicate, bool canCreate);
    //Task<UnitMembership> CreateMembership(UnitMembership membership);
    //Task<ListPermissionWrapper<UnitStatusType>> ListStatusTypes(Guid? unitId = null);
    Task<ItemPermissionWrapper<Animal>> GetAsync(Guid id);
    Task<ListPermissionWrapper<AnimalOwner>> ListOwners(Guid animalId);
    //Task DeleteStatusType(Guid statusTypeId);
    Task<AnimalOwner> SaveOwnership(AnimalOwner owner);
    Task DeleteOwnership(Guid ownershipId);

    //Task<UnitReportInfo[]> ListReports(Guid unitId);
    //Task<Unit> Save(Unit unit);
    //Task Delete(Guid unitId);
  }

  public class AnimalsService : IAnimalsService
  {
    private readonly Func<DB.IKcsarContext> _dbFactory;
    private readonly ILog _log;
    private readonly IAuthorizationService _authz;

    public AnimalsService(Func<DB.IKcsarContext> dbFactory, IAuthorizationService authz, ILog log)
    {
      _dbFactory = dbFactory;
      _authz = authz;
      _log = log;
    }

    public async Task<ListPermissionWrapper<Animal>> List()
    {
      using (var db = _dbFactory())
      {
        var list = await db.Animals.OrderBy(f => f.Name).Select(f => new Animal
        {
          Id = f.Id,
          Name = f.Name,
          Type = f.Type,
          Status = f.Owners.Any(g => !g.Ending.HasValue) ? "active": "inactive",
          Owner = f.Owners.Where(g => g.IsPrimary).Select(g => new MemberSummary
          {
            Id = g.Owner.Id,
            Name = g.Owner.LastName + ", " + g.Owner.FirstName,
            WorkerNumber = g.Owner.DEM
          }).FirstOrDefault()
        }).ToListAsync();

        return new ListPermissionWrapper<Animal>
        {
          C = _authz.CanCreate<Animal>(),
          Items = list.Select(f => _authz.Wrap(f))
        };
      }
    }
    
    public async Task<ItemPermissionWrapper<Animal>> GetAsync(Guid id)
    {
      using (var db = _dbFactory())
      {
        var result = await db.Animals.Select(f => new Animal
        {
          Id = f.Id,
          Name = f.Name,
          Type = f.Type,
          Status = f.Owners.Any(g => !g.Ending.HasValue) ? "active" : "inactive",
          Owner = f.Owners.Where(g => g.IsPrimary).Select(g => new MemberSummary
          {
            Id = g.Owner.Id,
            Name = g.Owner.LastName + ", " + g.Owner.FirstName,
            WorkerNumber = g.Owner.DEM
          }).FirstOrDefault(),
          Comments = f.Comments,
          IdSuffix = f.DemSuffix,
          Photo = f.PhotoFile
        }).SingleOrDefaultAsync(f => f.Id == id);

        return _authz.Wrap(result);
      }
    }


    public async Task<ListPermissionWrapper<AnimalOwner>> ListOwners(Guid animalId)
    {
      using (var db = _dbFactory())
      {
        var list = new ListPermissionWrapper<AnimalOwner>
        {
          C = _authz.CanCreateStatusForUnit(animalId),
          Items = (await db.AnimalOwners.Where(f => f.Animal.Id == animalId)
                  .OrderByDescending(f => f.IsPrimary)
                  .ThenBy(f => f.Owner.LastName)
                  .ThenBy(f => f.Owner.FirstName)
                  .Select(f => new AnimalOwner
                  {
                    Id = f.Id,
                    Member = new MemberSummary
                    {
                      Id = f.Owner.Id,
                      Name = f.Owner.LastName + ", " + f.Owner.FirstName,
                      WorkerNumber = f.Owner.DEM
                    },
                    Animal = new NameIdPair
                    {
                      Id = f.Animal.Id,
                      Name = f.Animal.Name
                    },
                    Starting = f.Starting,
                    Ending = f.Ending
                  })
                  .ToListAsync())
                  .Select(f => _authz.Wrap(f))
                  .ToList()
        };

        return list;
      }
    }

    public async Task<AnimalOwner> SaveOwnership(AnimalOwner owner)
    {
      using (var db = _dbFactory())
      {
        var match = await db.AnimalOwners.FirstOrDefaultAsync(
          f => f.Id != owner.Id &&
          f.Owner.Id == owner.Member.Id &&
          f.Animal.Id == owner.Animal.Id &&
          f.Starting == owner.Starting);
        if (match != null) throw new DuplicateItemException("AnimalOwner", owner.Id.ToString());

        var updater = await ObjectUpdater.CreateUpdater(
          db.AnimalOwners,
          owner.Id,
          f => {
            f.OwnerId = owner.Member.Id;
            f.Owner = db.Members.Single(g => g.Id == owner.Member.Id);
            f.AnimalId = owner.Animal.Id;
            f.Animal = db.Animals.Single(g => g.Id == owner.Animal.Id);
          });

        updater.Update(f => f.Starting, owner.Starting.UtcDateTime);
        updater.Update(f => f.Ending, owner.Ending?.UtcDateTime);
        updater.Update(f => f.IsPrimary, owner.IsPrimary);
        if (owner.IsPrimary)
        {
          foreach (var secondary in db.AnimalOwners.Where(f => f.Id != owner.Id && f.AnimalId == owner.Animal.Id))
          {
            secondary.IsPrimary = false;
          }
        }
        updater.Update(f => f.AnimalId, owner.Animal.Id);
        updater.Update(f => f.OwnerId, owner.Member.Id);

        await updater.Persist(db);

        return (await ListOwners(owner.Animal.Id)).Items.Single(f => f.Item.Id == updater.Instance.Id).Item;
      }
    }

    public async Task DeleteOwnership(Guid ownershipId)
    {
      using (var db = _dbFactory())
      {
        var ownership = await db.AnimalOwners.FirstOrDefaultAsync(f => f.Id == ownershipId);

        if (ownership == null) throw new NotFoundException("not found", "AnimalOwner", ownershipId.ToString());
        db.AnimalOwners.Remove(ownership);
        await db.SaveChangesAsync();
      }
    }
  }
}
