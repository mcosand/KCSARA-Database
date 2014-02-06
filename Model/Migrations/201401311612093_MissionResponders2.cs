namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissionResponders2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MissionResponderTimelimes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ResponderId = c.Guid(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        Location = c.Geography(),
                        Eta = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MissionResponders", t => t.ResponderId, cascadeDelete: true)
                .Index(t => t.ResponderId);
            
            AddColumn("dbo.MissionResponders", "LastTimelineId", c => c.Guid());
            CreateIndex("dbo.MissionResponders", "LastTimelineId");
            AddForeignKey("dbo.MissionResponders", "LastTimelineId", "dbo.MissionResponderTimelimes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MissionResponderTimelimes", "ResponderId", "dbo.MissionResponders");
            DropForeignKey("dbo.MissionResponders", "LastTimelineId", "dbo.MissionResponderTimelimes");
            DropIndex("dbo.MissionResponderTimelimes", new[] { "ResponderId" });
            DropIndex("dbo.MissionResponders", new[] { "LastTimelineId" });
            DropColumn("dbo.MissionResponders", "LastTimelineId");
            DropTable("dbo.MissionResponderTimelimes");
        }
    }
}
