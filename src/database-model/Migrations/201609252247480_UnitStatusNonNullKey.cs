namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnitStatusNonNullKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UnitStatus", "Unit_Id", "dbo.SarUnits");
            DropIndex("dbo.UnitStatus", new[] { "Unit_Id" });
            AlterColumn("dbo.UnitStatus", "Unit_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.UnitStatus", "Unit_Id");
            AddForeignKey("dbo.UnitStatus", "Unit_Id", "dbo.SarUnits", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UnitStatus", "Unit_Id", "dbo.SarUnits");
            DropIndex("dbo.UnitStatus", new[] { "Unit_Id" });
            AlterColumn("dbo.UnitStatus", "Unit_Id", c => c.Guid());
            CreateIndex("dbo.UnitStatus", "Unit_Id");
            AddForeignKey("dbo.UnitStatus", "Unit_Id", "dbo.SarUnits", "Id");
        }
    }
}
