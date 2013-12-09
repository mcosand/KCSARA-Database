namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnitDocuments : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UnitApplicationDocuments", "Unit_Id", "dbo.SarUnits");
            DropIndex("dbo.UnitApplicationDocuments", new[] { "Unit_Id" });
            CreateTable(
                "dbo.UnitDocuments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Url = c.String(nullable: false),
                        Title = c.String(nullable: false),
                        Order = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        SubmitTo = c.String(),
                        Required = c.Boolean(nullable: false),
                        ForMembersOlder = c.Int(),
                        ForMembersYounger = c.Int(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.MemberUnitDocuments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MemberAction = c.DateTime(),
                        UnitAction = c.DateTime(),
                        Status = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Member_Id = c.Guid(nullable: false),
                        Document_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Member_Id, cascadeDelete: true)
                .ForeignKey("dbo.UnitDocuments", t => t.Document_Id, cascadeDelete: true)
                .Index(t => t.Member_Id)
                .Index(t => t.Document_Id);
            
            DropTable("dbo.UnitApplicationDocuments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UnitApplicationDocuments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Url = c.String(nullable: false),
                        Title = c.String(nullable: false),
                        Order = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropIndex("dbo.MemberUnitDocuments", new[] { "Document_Id" });
            DropIndex("dbo.MemberUnitDocuments", new[] { "Member_Id" });
            DropIndex("dbo.UnitDocuments", new[] { "Unit_Id" });
            DropForeignKey("dbo.MemberUnitDocuments", "Document_Id", "dbo.UnitDocuments");
            DropForeignKey("dbo.MemberUnitDocuments", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.UnitDocuments", "Unit_Id", "dbo.SarUnits");
            DropTable("dbo.MemberUnitDocuments");
            DropTable("dbo.UnitDocuments");
            CreateIndex("dbo.UnitApplicationDocuments", "Unit_Id");
            AddForeignKey("dbo.UnitApplicationDocuments", "Unit_Id", "dbo.SarUnits", "Id");
        }
    }
}
