namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class UnifiedEventModel_1 : DbMigration
  {
    public override void Up()
    {
      CreateTable(
          "dbo.SarEvents",
          c => new
              {
                Id = c.Guid(nullable: false),
                Title = c.String(),
                County = c.String(),
                StateNumber = c.String(),
                CountyNumber = c.String(),
                MissionType = c.String(),
                StartTime = c.DateTime(nullable: false),
                StopTime = c.DateTime(),
                Comments = c.String(),
                Location = c.String(),
                ReportCompleted = c.Boolean(nullable: false),
                LastChanged = c.DateTime(nullable: false),
                ChangedBy = c.String(),
                Discriminator = c.String(nullable: false, maxLength: 128),
                Previous_Id = c.Guid(),
              })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.SarEvents", t => t.Previous_Id)
          .Index(t => t.Previous_Id);

      CreateTable(
          "dbo.Participants",
          c => new
              {
                Id = c.Guid(nullable: false),
                Firstname = c.String(),
                Lastname = c.String(),
                WorkerNumber = c.String(),
                EventId = c.Guid(nullable: false),
                MemberId = c.Guid(),
                LastChanged = c.DateTime(nullable: false),
                ChangedBy = c.String(),
              })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.SarEvents", t => t.EventId, cascadeDelete: true)
          .ForeignKey("dbo.Members", t => t.MemberId)
          .Index(t => t.EventId)
          .Index(t => t.MemberId);

      CreateTable(
          "dbo.EventRosters",
          c => new
              {
                Id = c.Guid(nullable: false),
                EventId = c.Guid(nullable: false),
                ParticipantId = c.Guid(nullable: false),
                UnitId = c.Guid(),
                Hours = c.Double(),
                Miles = c.Int(),
              })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.Participants", t => t.ParticipantId)
          .ForeignKey("dbo.ParticipatingUnits", t => t.UnitId)
          .ForeignKey("dbo.SarEvents", t => t.EventId)
          .Index(t => t.ParticipantId)
          .Index(t => t.UnitId)
          .Index(t => t.EventId);

      CreateTable(
          "dbo.ParticipatingUnits",
          c => new
              {
                Id = c.Guid(nullable: false),
                Nickname = c.String(),
                Name = c.String(),
                EventId = c.Guid(nullable: false),
                MemberUnitId = c.Guid(),
                LastChanged = c.DateTime(nullable: false),
                ChangedBy = c.String(),
              })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.SarEvents", t => t.EventId, cascadeDelete: true)
          .ForeignKey("dbo.SarUnits", t => t.MemberUnitId)
          .Index(t => t.EventId)
          .Index(t => t.MemberUnitId);

      CreateTable(
          "dbo.EventTimelines",
          c => new
              {
                Id = c.Guid(nullable: false),
                EventId = c.Guid(nullable: false),
                Time = c.DateTime(nullable: false),
                JsonData = c.String(),
                LastChanged = c.DateTime(nullable: false),
                ChangedBy = c.String(),
                ParticipantId = c.Guid(),
                UnitId = c.Guid(),
                Status = c.Int(),
                Discriminator = c.String(nullable: false, maxLength: 128),
              })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.SarEvents", t => t.EventId, cascadeDelete: true)
          .ForeignKey("dbo.Participants", t => t.ParticipantId)
          .ForeignKey("dbo.ParticipatingUnits", t => t.UnitId)
          .Index(t => t.EventId)
          .Index(t => t.ParticipantId)
          .Index(t => t.UnitId);

      CreateTable(
          "dbo.Training2TrainingCourse",
          c => new
              {
                Training2_Id = c.Guid(nullable: false),
                TrainingCourse_Id = c.Guid(nullable: false),
              })
          .PrimaryKey(t => new { t.Training2_Id, t.TrainingCourse_Id })
          .ForeignKey("dbo.SarEvents", t => t.Training2_Id, cascadeDelete: true)
          .ForeignKey("dbo.TrainingCourses", t => t.TrainingCourse_Id, cascadeDelete: true)
          .Index(t => t.Training2_Id)
          .Index(t => t.TrainingCourse_Id);

      // Copy the events
      Sql(@"INSERT INTO SarEvents (Id, Title, County, StateNumber, CountyNumber, MissionType, StartTime, StopTime, Comments, Location, ReportCompleted, LastChanged, Discriminator, Previous_Id)
SELECT Id,Title,County,StateNumber,CountyNumber,MissionType,StartTime,StopTime,Comments,Location,ReportCompleted, LastChanged,'Mission2',Previous_Id FROM Missions");

      Sql(@"INSERT INTO SarEvents (Id, Title, County, StateNumber, CountyNumber, MissionType, StartTime, StopTime, Comments, Location, ReportCompleted, LastChanged, Discriminator, Previous_Id)
SELECT Id,Title,County,StateNumber,null,null,StartTime,StopTime,Comments,Location, 0, LastChanged,'Training2',NULL FROM Trainings");

      // Copy the participating units
      Sql(@"INSERT INTO ParticipatingUnits (Id,Nickname,Name,EventId,MemberUnitId,LastChanged,ChangedBy)
SELECT NEWID(),u.DisplayName, u.LongName, mr.Mission_Id, u.id, GETDATE(), 'migration'
FROM SarUnits u JOIN MissionRosters mr ON u.id=mr.Unit_Id
GROUP BY u.DisplayName, u.LongName, mr.Mission_Id, u.id");

      // Copy the participants
      Sql(@"INSERT INTO Participants (Id,Firstname,Lastname,WorkerNumber, EventId,MemberId,LastChanged,ChangedBy)
SELECT NEWID(), m.FirstName, m.LastName, m.DEM, mr.Mission_Id, m.Id, GETDATE(), 'migration'
FROM Members m JOIN MissionRosters mr ON m.id=mr.Person_Id
GROUP BY m.FirstName, m.LastName, m.DEM, mr.Mission_Id, m.id");

      Sql(@"INSERT INTO Participants (Id,Firstname,Lastname,WorkerNumber, EventId,MemberId,LastChanged,ChangedBy)
SELECT NEWID(), m.FirstName, m.LastName, m.DEM, tr.Training_Id, m.Id, GETDATE(), 'migration'
FROM Members m JOIN TrainingRosters tr ON m.id=tr.Person_Id
GROUP BY m.FirstName, m.LastName, m.DEM, tr.Training_Id, m.id");

      // Copy the roster summaries
      Sql(@"INSERT INTO EventRosters (Id, EventId, ParticipantId, UnitId, Hours, Miles)
SELECT NEWID() AS Id, r.Mission_Id, p.Id AS MemberId, pu.Id AS UnitId, CASE WHEN COUNT(timein) = COUNT(timeout) THEN SUM(datediff(minute, timein, timeout)/60.0) ELSE NULL END AS Hours, CASE WHEN COUNT(Miles) > 0 THEN SUM(ISNULL(Miles, 0)) ELSE SUM(Miles) END AS Miles
FROM MissionRosters r
  JOIN Participants p ON r.Mission_Id = p.EventId AND r.Person_Id = p.MemberId
  JOIN ParticipatingUnits pu ON r.Mission_Id = pu.EventId AND r.Unit_Id = pu.MemberUnitId
GROUP BY r.Mission_Id, p.Id, pu.Id");
      Sql(@"INSERT INTO EventRosters (Id, EventId, ParticipantId, UnitId, Hours, Miles)
SELECT NEWID() AS Id, r.Training_Id, p.Id AS MemberId, NULL AS UnitId, CASE WHEN COUNT(timein) = COUNT(timeout) THEN SUM(datediff(minute, timein, timeout)/60.0) ELSE NULL END AS Hours, CASE WHEN COUNT(Miles) > 0 THEN SUM(ISNULL(Miles, 0)) ELSE SUM(Miles) END AS Miles
FROM TrainingRosters r
  JOIN Participants p ON r.Training_Id = p.EventId AND r.Person_Id = p.MemberId
GROUP BY r.Training_Id, p.Id");

      // Copy the roster timelines
      Sql(@"INSERT INTO EventTimelines (Id,EventId,Time,JsonData,LastChanged,ChangedBy,ParticipantId,UnitId,Status,Discriminator)
SELECT NEWID(),r.Mission_Id,r.TimeIn, NULL, r.LastChanged, r.ChangedBy, p.Id, pu.Id, 3, 'ParticipantStatus'
FROM MissionRosters r
  JOIN Participants p ON r.Mission_Id = p.EventId AND r.Person_Id = p.MemberId
  JOIN ParticipatingUnits pu ON r.Mission_Id = pu.EventId AND r.Unit_Id = pu.MemberUnitId
WHERE TimeIn IS NOT NULL
UNION
SELECT NEWID(),r.Mission_Id,r.TimeOut, NULL, r.LastChanged, r.ChangedBy, p.Id, pu.Id, 8, 'ParticipantStatus'
FROM MissionRosters r
  JOIN Participants p ON r.Mission_Id = p.EventId AND r.Person_Id = p.MemberId
  JOIN ParticipatingUnits pu ON r.Mission_Id = pu.EventId AND r.Unit_Id = pu.MemberUnitId
WHERE TimeOut IS NOT NULL");

      Sql(@"INSERT INTO EventTimelines (Id,EventId,Time,JsonData,LastChanged,ChangedBy,ParticipantId,UnitId,Status,Discriminator)
SELECT NEWID(),r.Training_Id,r.TimeIn, NULL, r.LastChanged, r.ChangedBy, p.Id, NULL, 3, 'ParticipantStatus'
FROM TrainingRosters r
  JOIN Participants p ON r.Training_Id = p.EventId AND r.Person_Id = p.MemberId
WHERE TimeIn IS NOT NULL
UNION
SELECT NEWID(),r.Training_Id,r.TimeOut, NULL, r.LastChanged, r.ChangedBy, p.Id, NULL, 8, 'ParticipantStatus'
FROM TrainingRosters r
  JOIN Participants p ON r.Training_Id = p.EventId AND r.Person_Id = p.MemberId
WHERE TimeOut IS NOT NULL");


      // Copy over the mission Logs
      Sql(@"INSERT INTO EventTimelines (Id,EventId,Time,JsonData,LastChanged,ChangedBy,ParticipantId,UnitId,Status,Discriminator)
SELECT l.Id,l.Mission_Id,l.Time,'""' + Replace(l.Data, '""', '\""') + '""', l.LastChanged,l.ChangedBy, p.id, NULL, NULL, 'EventLog'
FROM MissionLogs l JOIN Participants p ON l.Mission_Id = p.EventId and l.Person_Id = p.MemberId");

      // Populate courses offered at particular training events
      Sql(@"INSERT INTO Training2TrainingCourse (Training2_Id, TrainingCourse_Id)
SELECT Training_Id, TrainingCourse_Id FROM TrainingTrainingCourses");
    }

    public override void Down()
    {
      DropForeignKey("dbo.Training2TrainingCourse", "TrainingCourse_Id", "dbo.TrainingCourses");
      DropForeignKey("dbo.Training2TrainingCourse", "Training2_Id", "dbo.SarEvents");
      DropForeignKey("dbo.EventTimelines", "UnitId", "dbo.ParticipatingUnits");
      DropForeignKey("dbo.EventTimelines", "ParticipantId", "dbo.Participants");
      DropForeignKey("dbo.EventTimelines", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.EventRosters", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.EventRosters", "UnitId", "dbo.ParticipatingUnits");
      DropForeignKey("dbo.ParticipatingUnits", "MemberUnitId", "dbo.SarUnits");
      DropForeignKey("dbo.ParticipatingUnits", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.EventRosters", "ParticipantId", "dbo.Participants");
      DropForeignKey("dbo.Participants", "MemberId", "dbo.Members");
      DropForeignKey("dbo.Participants", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.SarEvents", "Previous_Id", "dbo.SarEvents");
      DropIndex("dbo.Training2TrainingCourse", new[] { "TrainingCourse_Id" });
      DropIndex("dbo.Training2TrainingCourse", new[] { "Training2_Id" });
      DropIndex("dbo.EventTimelines", new[] { "UnitId" });
      DropIndex("dbo.EventTimelines", new[] { "ParticipantId" });
      DropIndex("dbo.EventTimelines", new[] { "EventId" });
      DropIndex("dbo.EventRosters", new[] { "EventId" });
      DropIndex("dbo.EventRosters", new[] { "UnitId" });
      DropIndex("dbo.ParticipatingUnits", new[] { "MemberUnitId" });
      DropIndex("dbo.ParticipatingUnits", new[] { "EventId" });
      DropIndex("dbo.EventRosters", new[] { "ParticipantId" });
      DropIndex("dbo.Participants", new[] { "MemberId" });
      DropIndex("dbo.Participants", new[] { "EventId" });
      DropIndex("dbo.SarEvents", new[] { "Previous_Id" });
      DropTable("dbo.Training2TrainingCourse");
      DropTable("dbo.EventTimelines");
      DropTable("dbo.ParticipatingUnits");
      DropTable("dbo.EventRosters");
      DropTable("dbo.Participants");
      DropTable("dbo.SarEvents");
    }
  }
}
