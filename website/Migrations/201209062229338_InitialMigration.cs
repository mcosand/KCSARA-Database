namespace Kcsara.Database.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MeshNodeStatus",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Time = c.DateTime(nullable: false),
                        Location = c.Geography(),
                        IPAddr = c.String(),
                        Uptime = c.Single(nullable: false),
                        BatteryVolts = c.Single(nullable: false),
                        HouseVolts = c.Single(nullable: false),
                        AlternatorVolts = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.MeshNodeLocations",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                        Time = c.DateTime(nullable: false),
                        Location = c.Geography(nullable: false),
                    })
                .PrimaryKey(t => new { t.Name, t.Time });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MeshNodeLocations");
            DropTable("dbo.MeshNodeStatus");
        }
    }
}
