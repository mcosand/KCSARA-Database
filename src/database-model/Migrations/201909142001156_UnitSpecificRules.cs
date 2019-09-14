namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnitSpecificRules : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrainingRules", "UnitId", c => c.Guid());
            CreateIndex("dbo.TrainingRules", "UnitId");
            AddForeignKey("dbo.TrainingRules", "UnitId", "dbo.SarUnits", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingRules", "UnitId", "dbo.SarUnits");
            DropIndex("dbo.TrainingRules", new[] { "UnitId" });
            DropColumn("dbo.TrainingRules", "UnitId");
        }
    }
}
