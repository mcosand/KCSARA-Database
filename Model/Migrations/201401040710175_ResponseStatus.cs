namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResponseStatus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MissionResponseStatus",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CallForPeriod = c.DateTime(nullable: false),
                        StopStaging = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MissionResponseStatus", "Id", "dbo.Missions");
            DropIndex("dbo.MissionResponseStatus", new[] { "Id" });
            DropTable("dbo.MissionResponseStatus");
        }
    }
}
