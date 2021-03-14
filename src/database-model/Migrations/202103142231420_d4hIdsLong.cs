namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class d4hIdsLong : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Members", "D4HId", c => c.Long());
            AlterColumn("dbo.Trainings", "D4HId", c => c.Long());
            AlterColumn("dbo.TrainingCourses", "D4HId", c => c.Long(nullable: false));
            AlterColumn("dbo.TrainingRosters", "D4HId", c => c.Long());
            AlterColumn("dbo.TrainingAwards", "D4HId", c => c.Long(nullable: false));
            AlterColumn("dbo.MissionRosters", "D4HId", c => c.Long());
            AlterColumn("dbo.Missions", "D4HId", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Missions", "D4HId", c => c.Int());
            AlterColumn("dbo.MissionRosters", "D4HId", c => c.Int());
            AlterColumn("dbo.TrainingAwards", "D4HId", c => c.Int(nullable: false));
            AlterColumn("dbo.TrainingRosters", "D4HId", c => c.Int());
            AlterColumn("dbo.TrainingCourses", "D4HId", c => c.Int(nullable: false));
            AlterColumn("dbo.Trainings", "D4HId", c => c.Int());
            AlterColumn("dbo.Members", "D4HId", c => c.Int());
        }
    }
}
