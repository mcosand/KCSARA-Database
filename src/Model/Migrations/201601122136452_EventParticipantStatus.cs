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
 SELECT NEWID(),p.Id,TimeIn, 15, InternalRole, u.id, NULL, GETDATE(), 'dbupgrade' FROM TrainingRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Training_Id=p.EventId
 JOIN EventUnits u ON mr.Unit_Id=u.MemberUnitId AND mr.Training_Id=u.EventId");
      Sql(@"INSERT INTO [dbo].[EventParticipantStatus] ([Id],[ParticipantId],[Time],[Status],[Role],[EventUnitId],[Miles],[LastChanged],[ChangedBy])
 SELECT NEWID(),p.Id,[TimeOut], 259, NULL, NULL, mr.miles, GETDATE(), 'dbupgrade' FROM TrainingRosters mr
 join EventParticipants p ON mr.Person_Id=p.MemberId and mr.Training_Id=p.EventId
 WHERE [TimeOut] IS NOT NULL");
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
