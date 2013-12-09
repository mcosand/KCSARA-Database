namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissionRosterRequired : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits");
            DropIndex("dbo.MissionRosters", new[] { "Mission_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Person_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Unit_Id" });
            AlterColumn("dbo.MissionRosters", "Mission_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.MissionRosters", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.MissionRosters", "Unit_Id", c => c.Guid(nullable: false));
            AddForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits", "Id", cascadeDelete: true);
            CreateIndex("dbo.MissionRosters", "Mission_Id");
            CreateIndex("dbo.MissionRosters", "Person_Id");
            CreateIndex("dbo.MissionRosters", "Unit_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MissionRosters", new[] { "Unit_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Person_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Mission_Id" });
            DropForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions");
            AlterColumn("dbo.MissionRosters", "Unit_Id", c => c.Guid());
            AlterColumn("dbo.MissionRosters", "Person_Id", c => c.Guid());
            AlterColumn("dbo.MissionRosters", "Mission_Id", c => c.Guid());
            CreateIndex("dbo.MissionRosters", "Unit_Id");
            CreateIndex("dbo.MissionRosters", "Person_Id");
            CreateIndex("dbo.MissionRosters", "Mission_Id");
            AddForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits", "Id");
            AddForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions", "Id");
        }
    }
}
