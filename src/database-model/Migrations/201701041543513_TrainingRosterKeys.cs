namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingRosterKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings");
            DropIndex("dbo.TrainingRosters", new[] { "Person_Id" });
            DropIndex("dbo.TrainingRosters", new[] { "Training_Id" });
            AlterColumn("dbo.TrainingRosters", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.TrainingRosters", "Training_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.TrainingRosters", "Person_Id");
            CreateIndex("dbo.TrainingRosters", "Training_Id");
            AddForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings", "Id", cascadeDelete: true);


            Sql("IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.MissionRosters') AND NAME ='IX_PersonId_TimeIn')    DROP INDEX IX_PersonId_TimeIn ON dbo.MissionRosters;     CREATE INDEX IX_PersonId_TimeIn ON dbo.MissionRosters(Person_Id,TimeIn);");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings");
            DropForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members");
            DropIndex("dbo.TrainingRosters", new[] { "Training_Id" });
            DropIndex("dbo.TrainingRosters", new[] { "Person_Id" });
            AlterColumn("dbo.TrainingRosters", "Training_Id", c => c.Guid());
            AlterColumn("dbo.TrainingRosters", "Person_Id", c => c.Guid());
            CreateIndex("dbo.TrainingRosters", "Training_Id");
            CreateIndex("dbo.TrainingRosters", "Person_Id");
            AddForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings", "Id");
            AddForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members", "Id");
        }
    }
}
