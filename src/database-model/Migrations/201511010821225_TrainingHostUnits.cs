namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class TrainingHostUnits : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.Trainings", "HostUnitId", "dbo.SarUnits");
      DropIndex("dbo.Trainings", new[] { "HostUnitId" });
      CreateTable(
          "dbo.TrainingSarUnits",
          c => new
          {
            Training_Id = c.Guid(nullable: false),
            SarUnit_Id = c.Guid(nullable: false),
          })
          .PrimaryKey(t => new { t.Training_Id, t.SarUnit_Id })
          .ForeignKey("dbo.Trainings", t => t.Training_Id, cascadeDelete: true)
          .ForeignKey("dbo.SarUnits", t => t.SarUnit_Id, cascadeDelete: true)
          .Index(t => t.Training_Id)
          .Index(t => t.SarUnit_Id);
      Sql("INSERT INTO TrainingSarUnits (Training_Id, SarUnit_Id) SELECT Id, HostUnitId FROM Trainings WHERE HostUnitId IS NOT NULL");
      DropColumn("dbo.Trainings", "HostUnitId");
    }

    public override void Down()
    {
      AddColumn("dbo.Trainings", "HostUnitId", c => c.Guid());
      DropForeignKey("dbo.TrainingSarUnits", "SarUnit_Id", "dbo.SarUnits");
      DropForeignKey("dbo.TrainingSarUnits", "Training_Id", "dbo.Trainings");
      DropIndex("dbo.TrainingSarUnits", new[] { "SarUnit_Id" });
      DropIndex("dbo.TrainingSarUnits", new[] { "Training_Id" });
      DropTable("dbo.TrainingSarUnits");
      CreateIndex("dbo.Trainings", "HostUnitId");
      AddForeignKey("dbo.Trainings", "HostUnitId", "dbo.SarUnits", "Id");
    }
  }
}
