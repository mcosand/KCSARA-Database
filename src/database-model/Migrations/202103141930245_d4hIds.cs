namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class d4hIds : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "D4HId", c => c.Int());
            AddColumn("dbo.Trainings", "D4HId", c => c.Int());
            AddColumn("dbo.TrainingCourses", "D4HId", c => c.Int(nullable: false));
            AddColumn("dbo.TrainingRosters", "D4HId", c => c.Int());
            AddColumn("dbo.TrainingAwards", "D4HId", c => c.Int(nullable: false));
            AddColumn("dbo.MissionRosters", "D4HId", c => c.Int());
            AddColumn("dbo.Missions", "D4HId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Missions", "D4HId");
            DropColumn("dbo.MissionRosters", "D4HId");
            DropColumn("dbo.TrainingAwards", "D4HId");
            DropColumn("dbo.TrainingRosters", "D4HId");
            DropColumn("dbo.TrainingCourses", "D4HId");
            DropColumn("dbo.Trainings", "D4HId");
            DropColumn("dbo.Members", "D4HId");
        }
    }
}
