namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrainingHostUnit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trainings", "HostUnitId", c => c.Guid());
            CreateIndex("dbo.Trainings", "HostUnitId");
            AddForeignKey("dbo.Trainings", "HostUnitId", "dbo.SarUnits", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Trainings", "HostUnitId", "dbo.SarUnits");
            DropIndex("dbo.Trainings", new[] { "HostUnitId" });
            DropColumn("dbo.Trainings", "HostUnitId");
        }
    }
}
