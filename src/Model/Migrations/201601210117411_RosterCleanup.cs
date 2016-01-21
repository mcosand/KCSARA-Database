namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class RosterCleanup : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.ComputedTrainingAwards", "Roster_Id", "dbo.TrainingRosters");
      DropForeignKey("dbo.TrainingAwards", "Roster_Id", "dbo.TrainingRosters");
      DropForeignKey("dbo.EventRosters", "Event_Id", "dbo.Events");
      DropForeignKey("dbo.EventRosters", "Person_Id", "dbo.Members");
      DropIndex("dbo.ComputedTrainingAwards", new[] { "Roster_Id" });
      DropIndex("dbo.TrainingAwards", new[] { "Roster_Id" });
      DropIndex("dbo.EventRosters", new[] { "Event_Id" });
      DropIndex("dbo.EventRosters", new[] { "Person_Id" });
      AddColumn("dbo.TrainingAwards", "RosterId", c => c.Guid());
      AddColumn("dbo.ComputedTrainingAwards", "RosterId", c => c.Guid());
      CreateIndex("dbo.ComputedTrainingAwards", "RosterId");
      CreateIndex("dbo.TrainingAwards", "RosterId");

      Sql(@"UPDATE ta SET
 RosterId=ep.id
FROM
 EventParticipants ep 
 join TrainingRosters tr ON ep.EventId=tr.Training_Id AND ep.MemberId=tr.Person_Id 
 JOIN TrainingAwards ta ON ta.Roster_Id=tr.id");

      Sql(@"UPDATE cta SET
 RosterId=ep.id
FROM
 EventParticipants ep 
 join TrainingRosters tr ON ep.EventId=tr.Training_Id AND ep.MemberId=tr.Person_Id 
 JOIN ComputedTrainingAwards cta ON cta.Roster_Id=tr.id");


      AddForeignKey("dbo.TrainingAwards", "RosterId", "dbo.EventParticipants", "Id");
      AddForeignKey("dbo.ComputedTrainingAwards", "RosterId", "dbo.EventParticipants", "Id");
      DropColumn("dbo.TrainingAwards", "Roster_Id");
      DropColumn("dbo.ComputedTrainingAwards", "Roster_Id");
      DropTable("dbo.EventRosters");
    }

    public override void Down()
    {
      CreateTable(
          "dbo.EventRosters",
          c => new
          {
            Id = c.Guid(nullable: false),
            TimeIn = c.DateTime(),
            TimeOut = c.DateTime(),
            Miles = c.Int(),
            Comments = c.String(),
            OvertimeHours = c.Int(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
            Event_Id = c.Guid(),
            Person_Id = c.Guid(),
          })
          .PrimaryKey(t => t.Id);

      AddColumn("dbo.ComputedTrainingAwards", "Roster_Id", c => c.Guid());
      AddColumn("dbo.TrainingAwards", "Roster_Id", c => c.Guid());
      DropForeignKey("dbo.ComputedTrainingAwards", "RosterId", "dbo.EventParticipants");
      DropForeignKey("dbo.TrainingAwards", "RosterId", "dbo.EventParticipants");
      DropIndex("dbo.TrainingAwards", new[] { "RosterId" });
      DropIndex("dbo.ComputedTrainingAwards", new[] { "RosterId" });
      DropColumn("dbo.ComputedTrainingAwards", "RosterId");
      DropColumn("dbo.TrainingAwards", "RosterId");
      CreateIndex("dbo.EventRosters", "Person_Id");
      CreateIndex("dbo.EventRosters", "Event_Id");
      CreateIndex("dbo.TrainingAwards", "Roster_Id");
      CreateIndex("dbo.ComputedTrainingAwards", "Roster_Id");
      AddForeignKey("dbo.EventRosters", "Person_Id", "dbo.Members", "Id");
      AddForeignKey("dbo.EventRosters", "Event_Id", "dbo.Events", "Id");
      AddForeignKey("dbo.TrainingAwards", "Roster_Id", "dbo.TrainingRosters", "Id");
      AddForeignKey("dbo.ComputedTrainingAwards", "Roster_Id", "dbo.TrainingRosters", "Id");
    }
  }
}
