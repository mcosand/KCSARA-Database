using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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
    Task<ItemPermissionWrapper<Animal>> GetAsync(Guid id);
    Task<Animal> Save(Animal animal);
    Task Delete(Guid animalId);

    Task<ListPermissionWrapper<AnimalOwner>> ListOwners(Guid animalId);
    Task<AnimalOwner> SaveOwnership(AnimalOwner owner);
    Task DeleteOwnership(Guid ownershipId);

    Task<List<EventAttendance>> GetMissionList(Guid animalId);
    Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid animalId);
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

    public async Task<Animal> Save(Animal animal)
    {
      using (var db = _dbFactory())
      {
        var updater = await ObjectUpdater.CreateUpdater(
          db.Animals,
          animal.Id,
          null);

        updater.Update(f => f.Name, animal.Name);
        updater.Update(f => f.Type, animal.Type);
        updater.Update(f => f.Comments, animal.Comments);
        updater.Update(f => f.DemSuffix, animal.IdSuffix);
        updater.Update(f => f.PhotoFile, animal.Photo);

        await updater.Persist(db);

        return (await List()).Items.Single(f => f.Item.Id == updater.Instance.Id).Item;
      }
    }

    public async Task Delete(Guid animalId)
    {
      using (var db = _dbFactory())
      {
        var animal = await db.Animals.FirstOrDefaultAsync(f => f.Id == animalId);

        if (animal == null) throw new NotFoundException("not found", "Animal", animalId.ToString());
        db.Animals.Remove(animal);
        await db.SaveChangesAsync();
      }
    }

    public async Task<AttendanceStatistics<NameIdPair>> GetMissionStatistics(Guid animalId)
    {
      using (var db = _dbFactory())
      {
        var recent = DateTime.UtcNow.AddMonths(-12);
        var stats = await db.Animals
          .Where(f => f.Id == animalId)
           .Select(f => new AttendanceStatistics<NameIdPair>
           {
             Total = new AttendanceStatisticsPart
             {
               Hours = f.MissionRosters.Sum(g => SqlFunctions.DateDiff("minute", g.MissionRoster.TimeIn, g.MissionRoster.TimeOut) / 60.0) ?? 0.0,
               Count = f.MissionRosters.Select(g => g.MissionRoster.MissionId).Distinct().Count()
             },
             Recent = new AttendanceStatisticsPart
             {
               Hours = f.MissionRosters.Where(g => g.MissionRoster.Mission.StartTime > recent).Sum(g => SqlFunctions.DateDiff("minute", g.MissionRoster.TimeIn, g.MissionRoster.TimeOut) / 60.0) ?? 0.0,
               Count = f.MissionRosters.Where(g => g.MissionRoster.Mission.StartTime > recent).Select(g => g.MissionRoster.MissionId).Distinct().Count()
             },
             Earliest = f.MissionRosters.Min(g => g.MissionRoster.TimeIn),
             Responder = new MemberSummary
             {
               Id = f.Id,
               Name = f.Name
             }
           })
           .FirstOrDefaultAsync();

        return stats;
      }
    }

    public async Task<List<EventAttendance>> GetMissionList(Guid animalId)
    {
      using (var db = _dbFactory())
      {
        var list = await db.AnimalMissions
          .Where(f => f.AnimalId == animalId)
          .GroupBy(f => f.MissionRoster.Mission)
          .Select(f => new EventAttendance
          {
            Event = new EventSummary
            {
              Id = f.Key.Id,
              Name = f.Key.Title,
              Location = f.Key.Location,
              Start = f.Key.StartTime,
              StateNumber = f.Key.StateNumber,
              Stop = f.Key.StopTime
            },
            Hours = f.Sum(g => SqlFunctions.DateDiff("minute", g.MissionRoster.TimeIn, g.MissionRoster.TimeOut) / 60.0) ?? 0.0
          })
          .OrderByDescending(f => f.Event.Start)
          .ToListAsync();
        return list;
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
