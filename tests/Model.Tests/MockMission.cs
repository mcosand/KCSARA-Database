using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using Moq;

namespace Internal.Database.Model
{
  public class MockMissions
  {
    public static void Create(IKcsarContext db)
    {
      var mtnRsq = AddTemplateUnit(db, new SarUnit { DisplayName = "MtnRsq", LongName = "Local Mountain Rescue" });
      var dogs = AddTemplateUnit(db, new SarUnit { DisplayName = "Dogs", LongName = "Local Dog Team" });

      var marc = AddTemplateMember(db, new Member { FirstName = "Marc", LastName = "Rickenbacker", DEM = "5940", Username = "marc" });
      var ray = AddTemplateMember(db, new Member { FirstName = "Ray", LastName = "Langworthy", DEM = "5302" });
      var virgil = AddTemplateMember(db, new Member { FirstName = "Virgil", LastName = "Devin", DEM = "2059", Username = "virgil" });
      var bernard = AddTemplateMember(db, new Member { FirstName = "Bernard", LastName = "Daily", DEM = "8493", Username = "bernard" });

      marc.Username = "marc";
      if (marc.Memberships.Count() < 1)
      {
        marc.Memberships.Add(new UnitMembership { Activated = new DateTime(1995, 7, 15), Person = marc, Status = mtnRsq.StatusTypes.First(f => f.IsActive), Unit = mtnRsq });
      }
      if (marc.Memberships.Count() < 2)
      {
        marc.Memberships.Add(new UnitMembership { Activated = new DateTime(2003, 10, 5), Person = marc, Status = dogs.StatusTypes.First(f => f.IsActive), Unit = dogs });
      }

      if (virgil.Memberships.Count() < 1)
      {
        virgil.Memberships.Add(new UnitMembership { Activated = new DateTime(2013, 5, 6), Status = mtnRsq.StatusTypes.First(f => f.IsActive), Unit = mtnRsq });
      }

      if (bernard.Memberships.Count() < 1)
      {
        bernard.Memberships.Add(new UnitMembership { Activated = new DateTime(1999, 8, 14), Status = dogs.StatusTypes.First(f => f.IsActive), Unit = dogs });
      }

      var snowlake = AddTemplateMission(db, new Mission { Title = "Snow Lake", StartTime = DateTime.Now.AddDays(-1), Location = "Over the Rainbow", MissionType = "rescue" });

      if (snowlake.RespondingUnits.Count == 0)
      {
        snowlake.RespondingUnits.Add(new MissionRespondingUnit { IsActive = true, Unit = mtnRsq, UnitId = mtnRsq.Id, Mission = snowlake, MissionId = snowlake.Id });
      }

      var responseStatus = new MissionResponseStatus { Mission = snowlake, CallForPeriod = snowlake.StartTime, StopStaging = snowlake.StartTime.AddHours(2.5) };
      if (snowlake.ResponseStatus == null)
      {
        snowlake.ResponseStatus = responseStatus;
      }
    }

    private static Member AddTemplateMember(IKcsarContext db, Member template)
    {
      var member = db.Members.SingleOrDefault(f => f.FirstName.Equals(template.FirstName, StringComparison.OrdinalIgnoreCase));
      if (member == null)
      {
        member = template;
        db.Members.Add(member);
      }
      return member;
    }

    private static SarUnit AddTemplateUnit(IKcsarContext db, SarUnit template)
    {
      var unit = db.Units.SingleOrDefault(f => f.DisplayName.Equals(template.DisplayName, StringComparison.OrdinalIgnoreCase));
      if (unit == null)
      {
        unit = template;
        db.Units.Add(unit);
        var statuses = new[] {
          new UnitStatus { IsActive = true, StatusName = "Active", WacLevel = WacLevel.Field, GetsAccount = true },
          new UnitStatus { IsActive = false, StatusName = "Alumni", WacLevel = WacLevel.None, GetsAccount = false }
        };
        foreach (var status in statuses)
        {
          unit.StatusTypes.Add(status);
          db.UnitStatusTypes.Add(status);
        }

      }
      return unit;
    }

    private static Mission AddTemplateMission(IKcsarContext db, Mission template)
    {
      var mission = db.Missions.SingleOrDefault(f => f.Title.Equals(template.Title, StringComparison.OrdinalIgnoreCase));
      if (mission == null)
      {
        mission = template;
        db.Missions.Add(mission);
      }
      return mission;
    }
  }
}
