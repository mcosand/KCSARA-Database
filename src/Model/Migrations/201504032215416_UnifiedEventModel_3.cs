namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class UnifiedEventModel_3 : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters");
      DropForeignKey("dbo.Missions", "Previous_Id", "dbo.Missions");
      DropForeignKey("dbo.MissionLogs", "Person_Id", "dbo.Members");
      DropForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions");
      DropForeignKey("dbo.SubjectGroups", "Mission_Id", "dbo.Missions");
      DropForeignKey("dbo.ComputedTrainingAwards", "Roster_Id", "dbo.TrainingRosters");
      DropForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members");
      DropForeignKey("dbo.TrainingTrainingCourses", "Training_Id", "dbo.Trainings");
      DropForeignKey("dbo.TrainingTrainingCourses", "TrainingCourse_Id", "dbo.TrainingCourses");
      DropForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings");
      DropForeignKey("dbo.TrainingAwards", "Roster_Id", "dbo.TrainingRosters");
      DropForeignKey("dbo.MissionGeographies", "Mission_Id", "dbo.Missions");
      DropForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions");
      DropForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members");
      DropForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits");
      DropForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters");
      DropForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents");
      DropIndex("dbo.AnimalMissions", new[] { "MissionRoster_Id" });
      DropIndex("dbo.Missions", new[] { "Previous_Id" });
      DropIndex("dbo.MissionLogs", new[] { "Person_Id" });
      DropIndex("dbo.MissionLogs", new[] { "Mission_Id" });
      DropIndex("dbo.SubjectGroups", new[] { "Mission_Id" });
      DropIndex("dbo.ComputedTrainingAwards", new[] { "Roster_Id" });
      DropIndex("dbo.TrainingRosters", new[] { "Person_Id" });
      DropIndex("dbo.TrainingTrainingCourses", new[] { "Training_Id" });
      DropIndex("dbo.TrainingTrainingCourses", new[] { "TrainingCourse_Id" });
      DropIndex("dbo.TrainingRosters", new[] { "Training_Id" });
      DropIndex("dbo.TrainingAwards", new[] { "Roster_Id" });
      DropIndex("dbo.MissionGeographies", new[] { "Mission_Id" });
      DropIndex("dbo.MissionRosters", new[] { "Mission_Id" });
      DropIndex("dbo.MissionRosters", new[] { "Person_Id" });
      DropIndex("dbo.MissionRosters", new[] { "Unit_Id" });
      DropIndex("dbo.AnimalMissions", new[] { "RosterId" });
      DropIndex("dbo.SubjectGroups", new[] { "EventId" });
      DropIndex("dbo.MissionGeographies", new[] { "EventId" });
      AlterColumn("dbo.MissionGeographies", "EventId", c => c.Guid(nullable: false));
      AlterColumn("dbo.SubjectGroups", "EventId", c => c.Guid(nullable: false));
      CreateIndex("dbo.AnimalMissions", "RosterId");
      CreateIndex("dbo.SubjectGroups", "EventId");
      CreateIndex("dbo.MissionGeographies", "EventId");
      AddForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters", "Id", cascadeDelete: true);
      AddForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents", "Id", cascadeDelete: true);
      DropColumn("dbo.MissionGeographies", "Mission_Id");
      DropColumn("dbo.SubjectGroups", "Mission_Id");
      DropColumn("dbo.ComputedTrainingAwards", "Roster_Id");
      DropColumn("dbo.TrainingAwards", "Roster_Id");
      DropTable("dbo.MissionRosters");
      DropTable("dbo.Missions");
      DropTable("dbo.MissionLogs");
      DropTable("dbo.TrainingRosters");
      DropTable("dbo.Trainings");
      DropTable("dbo.xref_county_id");
      DropTable("dbo.TrainingTrainingCourses");

      RenameTable("dbo.MissionGeographies", "EventGeographies");
      RenameTable("dbo.AnimalMissions", "AnimalEvents");
      RenameTable("dbo.MissionDetails", "EventDetails");

      Sql(@"UPDATE SarEvents SET Discriminator=Replace(Discriminator,'2','')");

      DropForeignKey("dbo.Training2TrainingCourse", "Training2_Id", "dbo.SarEvents");
      DropIndex("dbo.Training2TrainingCourse", new[] { "Training2_Id" });
      RenameTable(name: "dbo.Training2TrainingCourse", newName: "TrainingTrainingCourses");
      RenameColumn("dbo.TrainingTrainingCourses", "Training2_Id", "Training_Id");
      CreateIndex("dbo.TrainingTrainingCourses", "Training_Id");
      AddForeignKey("dbo.TrainingTrainingCourses", "Training_Id", "dbo.SarEvents", "Id", cascadeDelete: true);
    }

    public override void Down()
    {
      DropForeignKey("dbo.TrainingTrainingCourses", "Training_Id", "dbo.SarEvents");
      DropIndex("dbo.TrainingTrainingCourses", new[] { "Training_Id" });
      RenameColumn("dbo.TrainingTrainingCourses", "Training_Id", "Training2_Id");
      RenameTable(name: "dbo.TrainingTrainingCourses", newName: "Training2TrainingCourse");
      CreateIndex("dbo.Training2TrainingCourse", "Training2_Id");
      AddForeignKey("dbo.Training2TrainingCourse", "Training2_Id", "dbo.SarEvents", "Id", cascadeDelete: true);

      Sql(@"UPDATE SarEvents SET Discriminator=Discriminator+'2'");

      RenameTable("dbo.EventGeographies", "MissionGeographies");
      RenameTable("dbo.AnimalEvents", "AnimalMissions");
      RenameTable("dbo.EventDetails", "MissionDetails");

      CreateTable(
          "dbo.TrainingTrainingCourses",
          c => new
          {
            Training_Id = c.Guid(nullable: false),
            TrainingCourse_Id = c.Guid(nullable: false),
          })
          .PrimaryKey(t => new { t.Training_Id, t.TrainingCourse_Id });

      CreateTable(
          "dbo.xref_county_id",
          c => new
          {
            accessMemberID = c.Int(nullable: false),
            personId = c.Guid(nullable: false),
            ExternalSource = c.String(nullable: false, maxLength: 128),
          })
          .PrimaryKey(t => new { t.accessMemberID, t.personId, t.ExternalSource });

      CreateTable(
          "dbo.Trainings",
          c => new
          {
            Id = c.Guid(nullable: false),
            Title = c.String(nullable: false),
            County = c.String(),
            StateNumber = c.String(),
            StartTime = c.DateTime(nullable: false),
            StopTime = c.DateTime(),
            Previous = c.Guid(),
            Comments = c.String(),
            Location = c.String(nullable: false),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
          })
          .PrimaryKey(t => t.Id);

      CreateTable(
          "dbo.TrainingRosters",
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
            Person_Id = c.Guid(),
            Training_Id = c.Guid(),
          })
          .PrimaryKey(t => t.Id);

      CreateTable(
          "dbo.MissionLogs",
          c => new
          {
            Id = c.Guid(nullable: false),
            Time = c.DateTime(nullable: false),
            Data = c.String(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
            Person_Id = c.Guid(),
            Mission_Id = c.Guid(nullable: false),
          })
          .PrimaryKey(t => t.Id);

      CreateTable(
          "dbo.Missions",
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
            Previous_Id = c.Guid(),
          })
          .PrimaryKey(t => t.Id);

      CreateTable(
          "dbo.MissionRosters",
          c => new
          {
            Id = c.Guid(nullable: false),
            InternalRole = c.String(),
            TimeIn = c.DateTime(),
            TimeOut = c.DateTime(),
            Miles = c.Int(),
            Comments = c.String(),
            OvertimeHours = c.Double(),
            LastChanged = c.DateTime(nullable: false),
            ChangedBy = c.String(),
            Mission_Id = c.Guid(nullable: false),
            Person_Id = c.Guid(nullable: false),
            Unit_Id = c.Guid(nullable: false),
          })
          .PrimaryKey(t => t.Id);

      AddColumn("dbo.TrainingAwards", "Roster_Id", c => c.Guid());
      AddColumn("dbo.ComputedTrainingAwards", "Roster_Id", c => c.Guid());
      AddColumn("dbo.SubjectGroups", "Mission_Id", c => c.Guid());
      AddColumn("dbo.MissionGeographies", "Mission_Id", c => c.Guid());
      DropForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents");
      DropForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters");
      DropIndex("dbo.MissionGeographies", new[] { "EventId" });
      DropIndex("dbo.SubjectGroups", new[] { "EventId" });
      DropIndex("dbo.AnimalMissions", new[] { "RosterId" });
      AlterColumn("dbo.SubjectGroups", "EventId", c => c.Guid());
      AlterColumn("dbo.MissionGeographies", "EventId", c => c.Guid());
      CreateIndex("dbo.MissionGeographies", "EventId");
      CreateIndex("dbo.SubjectGroups", "EventId");
      CreateIndex("dbo.AnimalMissions", "RosterId");
      CreateIndex("dbo.MissionRosters", "Unit_Id");
      CreateIndex("dbo.MissionRosters", "Person_Id");
      CreateIndex("dbo.MissionRosters", "Mission_Id");
      CreateIndex("dbo.MissionGeographies", "Mission_Id");
      CreateIndex("dbo.TrainingAwards", "Roster_Id");
      CreateIndex("dbo.TrainingRosters", "Training_Id");
      CreateIndex("dbo.TrainingTrainingCourses", "TrainingCourse_Id");
      CreateIndex("dbo.TrainingTrainingCourses", "Training_Id");
      CreateIndex("dbo.TrainingRosters", "Person_Id");
      CreateIndex("dbo.ComputedTrainingAwards", "Roster_Id");
      CreateIndex("dbo.SubjectGroups", "Mission_Id");
      CreateIndex("dbo.MissionLogs", "Mission_Id");
      CreateIndex("dbo.MissionLogs", "Person_Id");
      CreateIndex("dbo.Missions", "Previous_Id");
      CreateIndex("dbo.AnimalMissions", "MissionRoster_Id");
      AddForeignKey("dbo.MissionGeographies", "EventId", "dbo.SarEvents", "Id");
      AddForeignKey("dbo.SubjectGroups", "EventId", "dbo.SarEvents", "Id");
      AddForeignKey("dbo.AnimalMissions", "RosterId", "dbo.EventRosters", "Id");
      AddForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionGeographies", "Mission_Id", "dbo.Missions", "Id");
      AddForeignKey("dbo.TrainingAwards", "Roster_Id", "dbo.TrainingRosters", "Id");
      AddForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings", "Id");
      AddForeignKey("dbo.TrainingTrainingCourses", "TrainingCourse_Id", "dbo.TrainingCourses", "Id", cascadeDelete: true);
      AddForeignKey("dbo.TrainingTrainingCourses", "Training_Id", "dbo.Trainings", "Id", cascadeDelete: true);
      AddForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members", "Id");
      AddForeignKey("dbo.ComputedTrainingAwards", "Roster_Id", "dbo.TrainingRosters", "Id");
      AddForeignKey("dbo.SubjectGroups", "Mission_Id", "dbo.Missions", "Id");
      AddForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions", "Id", cascadeDelete: true);
      AddForeignKey("dbo.MissionLogs", "Person_Id", "dbo.Members", "Id");
      AddForeignKey("dbo.Missions", "Previous_Id", "dbo.Missions", "Id");
      AddForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters", "Id");
    }
  }
}
