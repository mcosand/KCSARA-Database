namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MissionResponders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MissionRespondingUnits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MissionId = c.Guid(nullable: false),
                        UnitId = c.Guid(nullable: false),
                        LeadId = c.Guid(),
                        Name = c.String(),
                        LongName = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MissionResponders", t => t.LeadId)
                .ForeignKey("dbo.Missions", t => t.MissionId, cascadeDelete: true)
                .ForeignKey("dbo.SarUnits", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.LeadId)
                .Index(t => t.MissionId)
                .Index(t => t.UnitId);
            
            CreateTable(
                "dbo.MissionResponders",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MissionId = c.Guid(nullable: false),
                        RespondingUnitId = c.Guid(nullable: false),
                        MemberId = c.Guid(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        WorkerNumber = c.String(),
                        Hours = c.Decimal(precision: 18, scale: 2),
                        Miles = c.Int(),
                        Role = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.MemberId)
                .ForeignKey("dbo.Missions", t => t.MissionId)
                .ForeignKey("dbo.MissionRespondingUnits", t => t.RespondingUnitId, cascadeDelete: true)
                .Index(t => t.MemberId)
                .Index(t => t.MissionId)
                .Index(t => t.RespondingUnitId);
            
            AddColumn("dbo.MissionRosters", "Responder_Id", c => c.Guid());
            CreateIndex("dbo.MissionRosters", "Responder_Id");
            AddForeignKey("dbo.MissionRosters", "Responder_Id", "dbo.MissionResponders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MissionRespondingUnits", "UnitId", "dbo.SarUnits");
            DropForeignKey("dbo.MissionResponders", "RespondingUnitId", "dbo.MissionRespondingUnits");
            DropForeignKey("dbo.MissionRespondingUnits", "MissionId", "dbo.Missions");
            DropForeignKey("dbo.MissionRespondingUnits", "LeadId", "dbo.MissionResponders");
            DropForeignKey("dbo.MissionRosters", "Responder_Id", "dbo.MissionResponders");
            DropForeignKey("dbo.MissionResponders", "MissionId", "dbo.Missions");
            DropForeignKey("dbo.MissionResponders", "MemberId", "dbo.Members");
            DropIndex("dbo.MissionRespondingUnits", new[] { "UnitId" });
            DropIndex("dbo.MissionResponders", new[] { "RespondingUnitId" });
            DropIndex("dbo.MissionRespondingUnits", new[] { "MissionId" });
            DropIndex("dbo.MissionRespondingUnits", new[] { "LeadId" });
            DropIndex("dbo.MissionRosters", new[] { "Responder_Id" });
            DropIndex("dbo.MissionResponders", new[] { "MissionId" });
            DropIndex("dbo.MissionResponders", new[] { "MemberId" });
            DropColumn("dbo.MissionRosters", "Responder_Id");
            DropTable("dbo.MissionResponders");
            DropTable("dbo.MissionRespondingUnits");
        }
    }
}
