/*
 * Copyright 2016 Matthew Cosand
 */
 namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class EventParticipantStatus : DbMigration
  {
    public override void Up()
    {
      CreateTable(
          "dbo.EventParticipantStatus",
          c => new
          {
            Id = c.Guid(nullable: false),
            ParticipantId = c.Guid(nullable: false),
            Time = c.DateTime(nullable: false),
            Status = c.Int(nullable: false),
            Role = c.String(),
            EventUnitId = c.Guid(),
            Miles = c.Int(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
          })
          .PrimaryKey(t => t.Id)
          .ForeignKey("dbo.EventUnits", t => t.EventUnitId)
          .ForeignKey("dbo.EventParticipants", t => t.ParticipantId, cascadeDelete: true)
          .Index(t => t.ParticipantId)
          .Index(t => t.EventUnitId);

      Sql(@"INSERT INTO [dbo].[EventParticipantStatus] ([Id],[ParticipantId],[Time],[Status],[Role],[EventUnitId],[Miles],[LastChanged],[ChangedBy])
 SELECT NEWID(),p.Id,TimeIn, 15, InternalRole, u.id, NULL, GETDATE(), 'dbupgrade' FROM MissionRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Mission_Id=p.EventId
 JOIN EventUnits u ON mr.Unit_Id=u.MemberUnitId AND mr.Mission_Id=u.EventId");
      Sql(@"INSERT INTO [dbo].[EventParticipantStatus] ([Id],[ParticipantId],[Time],[Status],[Role],[EventUnitId],[Miles],[LastChanged],[ChangedBy])
 SELECT NEWID(),p.Id,[TimeOut], 259, NULL, NULL, mr.miles, GETDATE(), 'dbupgrade' FROM MissionRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Mission_Id=p.EventId
 WHERE [TimeOut] IS NOT NULL");
      Sql(@"INSERT INTO [dbo].[EventParticipantStatus] ([Id],[ParticipantId],[Time],[Status],[Role],[EventUnitId],[Miles],[LastChanged],[ChangedBy])
 SELECT NEWID(),p.Id,TimeIn, 15, NULL, NULL, NULL, GETDATE(), 'dbupgrade' FROM TrainingRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Training_Id=p.EventId");
      Sql(@"INSERT INTO [dbo].[EventParticipantStatus] ([Id],[ParticipantId],[Time],[Status],[Role],[EventUnitId],[Miles],[LastChanged],[ChangedBy])
 SELECT NEWID(),p.Id,[TimeOut], 259, NULL, NULL, mr.miles, GETDATE(), 'dbupgrade' FROM TrainingRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Training_Id=p.EventId
 WHERE [TimeOut] IS NOT NULL");

      Sql(@"CREATE NONCLUSTERED INDEX IX_ParticipantStats ON [dbo].[EventParticipants] ([MemberId]) INCLUDE ([EventId],[Hours],[Miles])");
      Sql(@"CREATE PROCEDURE EventDashboardStatistics
  @discriminator NVARCHAR(50)
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @thisYear DATETIME = DATEADD(YEAR, DATEDIFF(YEAR, '19000101', GETDATE()), '19000101')
  DECLARE @lastYear DATETIME = DATEADD(YEAR, -1, @thisYear)
  DECLARE @trend DATETIME = DATEADD(year, -10, @thisYear)
  
  SELECT COUNT(DISTINCT e.Id) as [Count], COUNT(DISTINCT ep.MemberId) as People, SUM(ISNULL(ep.Hours, 0)) as Hours, SUM(ISNULL(ep.Miles, 0)) as Miles
  FROM Events e LEFT JOIN EventParticipants ep ON ep.EventId=e.Id AND ep.MemberId IS NOT NULL
  WHERE e.StartTime > @thisYear AND e.Discriminator=@discriminator
  UNION
  SELECT COUNT(DISTINCT e.Id), COUNT(DISTINCT ep.MemberId), SUM(ISNULL(ep.Hours, 0)), SUM(ISNULL(ep.Miles, 0))
  FROM Events e LEFT JOIN EventParticipants ep ON ep.EventId=e.Id AND ep.MemberId IS NOT NULL
  WHERE e.StartTime > @lastYear AND e.StartTime < @thisYear AND e.Discriminator=@discriminator
  UNION
  SELECT AVG(Number), AVG(Participants), AVG(Hours), AVG(Miles) FROM (
    SELECT DATEPART(year, startTime) as [year], COUNT(DISTINCT e.id) as Number, COUNT(DISTINCT ep.Memberid) as Participants, SUM(ep.Hours) as Hours, SUM(ep.Miles) as Miles
    From Events e LEFT JOIN EventParticipants ep ON ep.EventId=e.Id AND ep.MemberId IS NOT NULL
    WHERE e.StartTime > @trend AND e.Discriminator=@discriminator
    AND e.StartTime < @thisYear
    GROUP BY DATEPART(year, startTime)
  ) b
END
GO");
    }

    public override void Down()
    {
      DropForeignKey("dbo.EventParticipantStatus", "ParticipantId", "dbo.EventParticipants");
      DropForeignKey("dbo.EventParticipantStatus", "EventUnitId", "dbo.EventUnits");
      DropIndex("dbo.EventParticipantStatus", new[] { "EventUnitId" });
      DropIndex("dbo.EventParticipantStatus", new[] { "ParticipantId" });
      DropTable("dbo.EventParticipantStatus");
    }
  }
}
