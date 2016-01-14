namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class EventParticipants : DbMigration
  {
    public override void Up()
    {
      CreateTable(
          "dbo.EventUnits",
          c => new
          {
            Id = c.Guid(nullable: false),
            Name = c.String(),
            County = c.String(),
            MemberUnitId = c.Guid(),
            EventId = c.Guid(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
          })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.Events", t => t.EventId)
          .ForeignKey("dbo.SarUnits", t => t.MemberUnitId)
          .Index(t => t.MemberUnitId)
          .Index(t => t.EventId);

      CreateTable(
          "dbo.EventParticipants",
          c => new
          {
            Id = c.Guid(nullable: false),
            EventId = c.Guid(nullable: false),
            FirstName = c.String(),
            LastName = c.String(),
            StateIdNumber = c.String(),
            MemberId = c.Guid(),
            EventUnitId = c.Guid(),
            LastStatus = c.Int(nullable: false),
            Hours = c.Double(),
            Miles = c.Int(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
          })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
          .ForeignKey("dbo.EventUnits", t => t.EventUnitId)
          .ForeignKey("dbo.Members", t => t.MemberId)
          .Index(t => t.EventId)
          .Index(t => t.MemberId)
          .Index(t => t.EventUnitId);

      Sql(@"INSERT INTO EventUnits (Id, Name, County, MemberUnitId, EventId, LastChanged, ChangedBy)
SELECT NEWID(), DisplayName, County, su.Id, mr.Mission_Id, GETDATE(), 'dbupgrade'
FROM MissionRosters mr JOIN SarUnits su ON mr.Unit_Id=su.Id GROUP BY DisplayName, County, su.id, mr.Mission_Id");


      Sql(@"INSERT INTO EventParticipants(Id, EventId, FirstName, LastName, StateIdNumber, MemberId, LastStatus, Hours, Miles, LastChanged, ChangedBy)
SELECT DISTINCT NEWID(), mr.Mission_id, m.FirstName, m.LastName, m.DEM, mr.person_id, -1, SUM(DATEDIFF(minute, timein, timeout) / 60.0), SUM(miles), getdate(), 'dbupgrade'
FROM MissionRosters mr JOIN Members m ON mr.Person_Id = m.Id
GROUP BY mr.Mission_Id,mr.Person_Id,m.firstname,m.lastname,m.dem");

      Sql(@"UPDATE p SET EventUnitId = eu.id, LastStatus = CASE WHEN[timeout] is null THEN 31 ELSE 259 END
   FROM EventParticipants p JOIN(
     SELECT ROW_NUMBER() OVER(PARTITION BY mr.mission_id, mr.person_id ORDER BY timein DESC) as r, mr.Mission_Id, mr.Unit_Id, mr.person_id, m.firstName, m.lastname, timein,[timeout] FROM MissionRosters mr JOIN Members m ON m.id = mr.Person_Id
   ) z ON z.r = 1 AND p.MemberId = z.Person_Id and p.EventId = z.mission_id
JOIN EventUnits eu ON z.Mission_Id = eu.EventId AND z.unit_id = eu.MemberUnitId");


      Sql(@"INSERT INTO EventParticipants(Id, EventId, FirstName, LastName, StateIdNumber, MemberId, LastStatus, Hours, Miles, LastChanged, ChangedBy)
SELECT DISTINCT NEWID(), mr.Training_id, m.FirstName, m.LastName, m.DEM, mr.person_id, -1, SUM(DATEDIFF(minute, timein, timeout) / 60.0), SUM(miles), getdate(), 'dbupgrade'
FROM TrainingRosters mr JOIN Members m ON mr.Person_Id = m.Id
GROUP BY mr.Training_Id,mr.Person_Id,m.firstname,m.lastname,m.dem");

      Sql(@"UPDATE p SET LastStatus = CASE WHEN[timeout] is null THEN 31 ELSE 259 END
 FROM EventParticipants p JOIN(
   SELECT ROW_NUMBER() OVER(PARTITION BY mr.Training_id, mr.person_id ORDER BY timein DESC) as r, mr.Training_Id, mr.person_id, m.firstName, m.lastname, timein,[timeout] FROM TrainingRosters mr JOIN Members m ON m.id = mr.Person_Id
 ) z ON z.r = 1 AND p.MemberId = z.Person_Id and p.EventId = z.Training_id");
    }

    public override void Down()
    {
      DropForeignKey("dbo.EventUnits", "MemberUnitId", "dbo.SarUnits");
      DropForeignKey("dbo.EventUnits", "EventId", "dbo.Events");
      DropForeignKey("dbo.EventParticipants", "MemberId", "dbo.Members");
      DropForeignKey("dbo.EventParticipants", "EventUnitId", "dbo.EventUnits");
      DropForeignKey("dbo.EventParticipants", "EventId", "dbo.Events");
      DropIndex("dbo.EventParticipants", new[] { "EventUnitId" });
      DropIndex("dbo.EventParticipants", new[] { "MemberId" });
      DropIndex("dbo.EventParticipants", new[] { "EventId" });
      DropIndex("dbo.EventUnits", new[] { "EventId" });
      DropIndex("dbo.EventUnits", new[] { "MemberUnitId" });
      DropTable("dbo.EventParticipants");
      DropTable("dbo.EventUnits");
    }
  }
}
