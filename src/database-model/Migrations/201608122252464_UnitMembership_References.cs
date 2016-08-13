namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnitMembership_References : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UnitMemberships", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.UnitMemberships", "Status_Id", "dbo.UnitStatus");
            DropIndex("dbo.UnitMemberships", new[] { "Status_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Unit_Id" });
            AlterColumn("dbo.UnitMemberships", "Status_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.UnitMemberships", "Unit_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.UnitMemberships", "Unit_Id");
            CreateIndex("dbo.UnitMemberships", "Status_Id");
            AddForeignKey("dbo.UnitMemberships", "Unit_Id", "dbo.SarUnits", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UnitMemberships", "Status_Id", "dbo.UnitStatus", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UnitMemberships", "Status_Id", "dbo.UnitStatus");
            DropForeignKey("dbo.UnitMemberships", "Unit_Id", "dbo.SarUnits");
            DropIndex("dbo.UnitMemberships", new[] { "Status_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Unit_Id" });
            AlterColumn("dbo.UnitMemberships", "Unit_Id", c => c.Guid());
            AlterColumn("dbo.UnitMemberships", "Status_Id", c => c.Guid());
            CreateIndex("dbo.UnitMemberships", "Unit_Id");
            CreateIndex("dbo.UnitMemberships", "Status_Id");
            AddForeignKey("dbo.UnitMemberships", "Status_Id", "dbo.UnitStatus", "Id");
            AddForeignKey("dbo.UnitMemberships", "Unit_Id", "dbo.SarUnits", "Id");
        }
    }
}
