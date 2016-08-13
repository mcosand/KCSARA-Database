namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Tracks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tracks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Username = c.String(),
                        Name = c.String(),
                        TripId = c.String(),
                        StartTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackPoints",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TrackId = c.Guid(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Point = c.Geography(),
                        Index = c.Int(nullable: false),
                        HAccuracy = c.Double(),
                        VAccuracy = c.Double(),
                        Altitude = c.Double(),
                        Battery = c.Double(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tracks", t => t.TrackId, cascadeDelete: true)
                .Index(t => t.TrackId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrackPoints", "TrackId", "dbo.Tracks");
            DropIndex("dbo.TrackPoints", new[] { "TrackId" });
            DropTable("dbo.TrackPoints");
            DropTable("dbo.Tracks");
        }
    }
}
