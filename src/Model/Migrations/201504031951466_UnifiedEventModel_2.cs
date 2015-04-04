namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class UnifiedEventModel_2 : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.MissionDetails", "Id", "dbo.Missions");
      DropIndex("dbo.MissionDetails", new[] { "Id" });
      AddColumn("dbo.AnimalMissions", "RosterId", c => c.Guid());
      AddColumn("dbo.MissionGeographies", "EventId", c => c.Guid());
      AddColumn("dbo.SubjectGroups", "EventId", c => c.Guid());
      AddColumn("dbo.ComputedTrainingAwards", "AttendanceId", c => c.Guid());
      AddColumn("dbo.TrainingAwards", "AttendanceId", c => c.Guid());
      CreateIndex("dbo.MissionDetails", "Id");
      CreateIndex("dbo.MissionGeographies", "EventId");
      CreateIndex("dbo.SubjectGroups", "EventId");
      CreateIndex("dbo.ComputedTrainingAwards", "AttendanceId");
      CreateIndex("dbo.TrainingAwards", "AttendanceId");
      CreateIndex("dbo.AnimalMissions", "RosterId");

      Sql(@"UPDATE AnimalMissions SET RosterId = er.Id
FROM EventRosters er JOIN Participants p ON er.ParticipantId=p.Id JOIN MissionRosters mr ON mr.Mission_Id=er.EventId AND mr.Person_Id=p.MemberId");
      Sql(@"UPDATE dbo.MissionGeographies SET EventId = Mission_Id");
      Sql(@"UPDATE dbo.SubjectGroups SET EventId = Mission_Id");
      Sql(@"UPDATE ComputedTrainingAwards
SET AttendanceId=p.Id
FROM ComputedTrainingAwards cta JOIN TrainingRosters tr ON cta.Roster_Id=tr.id JOIN Participants p ON cta.Member_Id = p.MemberId AND tr.Training_Id = p.eventId");
      Sql(@"UPDATE TrainingAwards
SET AttendanceId=p.Id
FROM TrainingAwards cta JOIN TrainingRosters tr ON cta.Roster_Id=tr.id JOIN Participants p ON cta.Member_Id = p.MemberId AND tr.Training_Id = p.eventId");

      AddForeignKey("dbo.MissionDetails", "Id", "dbo.SarEvents", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents", "Id");
      AddForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents", "Id");
      AddForeignKey("dbo.ComputedTrainingAwards", "AttendanceId", "dbo.Participants", "Id");
      AddForeignKey("dbo.TrainingAwards", "AttendanceId", "dbo.Participants", "Id");
      AddForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters", "Id");
    }

    public override void Down()
    {
      DropForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters");
      DropForeignKey("dbo.TrainingAwards", "AttendanceId", "dbo.Participants");
      DropForeignKey("dbo.ComputedTrainingAwards", "AttendanceId", "dbo.Participants");
      DropForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.MissionDetails", "Id", "dbo.SarEvents");
      DropIndex("dbo.AnimalMissions", new[] { "RosterId" });
      DropIndex("dbo.TrainingAwards", new[] { "AttendanceId" });
      DropIndex("dbo.ComputedTrainingAwards", new[] { "AttendanceId" });
      DropIndex("dbo.SubjectGroups", new[] { "EventId" });
      DropIndex("dbo.MissionGeographies", new[] { "EventId" });
      DropIndex("dbo.MissionDetails", new[] { "Id" });
      DropColumn("dbo.TrainingAwards", "AttendanceId");
      DropColumn("dbo.ComputedTrainingAwards", "AttendanceId");
      DropColumn("dbo.SubjectGroups", "EventId");
      DropColumn("dbo.MissionGeographies", "EventId");
      DropColumn("dbo.AnimalMissions", "RosterId");
      CreateIndex("dbo.MissionDetails", "Id");
      AddForeignKey("dbo.MissionDetails", "Id", "dbo.Missions", "Id", cascadeDelete: true);
    }
  }
}
