namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MemberStatus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemberMedicals",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EncryptedAllergies = c.String(),
                        EncryptedMedications = c.String(),
                        EncryptedDisclosures = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Id)
                .Index(t => t.Id);
            
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.UnitContacts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        Value = c.String(),
                        Purpose = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.UnitApplicants",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Started = c.DateTime(nullable: false),
                        Data = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Applicant_Id = c.Guid(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Applicant_Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Applicant_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.MemberEmergencyContacts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EncryptedData = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Member_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Member_Id)
                .Index(t => t.Member_Id);
            
            CreateTable(
                "dbo.SensitiveInfoAccesses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Actor = c.String(),
                        Timestamp = c.DateTime(nullable: false),
                        Action = c.String(),
                        Reason = c.String(),
                        Owner_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Owner_Id)
                .Index(t => t.Owner_Id);

            AddColumn("dbo.Members", "Status", c => c.Int(nullable: false, defaultValue: 1));
            AddColumn("dbo.SarUnits", "NoApplicationsText", c => c.String(nullable: false, defaultValue: ""));

            Sql(string.Format("update Members set Status={0}", (int)Model.MemberStatus.None));
            Sql(string.Format("update Members set Status={0} WHERE id IN (SELECT person_id from UnitMemberships um join UnitStatus us ON um.Status_Id=us.Id WHERE us.IsActive = 1 AND um.EndTime is null)", (int)Model.MemberStatus.Member));
            Sql(string.Format("update members set status={0} where Status={1} AND InternalWacLevel>0", (int)Model.MemberStatus.Mission, (int)Model.MemberStatus.Member));
        }
        
        public override void Down()
        {
            DropIndex("dbo.SensitiveInfoAccesses", new[] { "Owner_Id" });
            DropIndex("dbo.MemberEmergencyContacts", new[] { "Member_Id" });
            DropIndex("dbo.UnitApplicants", new[] { "Unit_Id" });
            DropIndex("dbo.UnitApplicants", new[] { "Applicant_Id" });
            DropIndex("dbo.UnitContacts", new[] { "Unit_Id" });
            DropIndex("dbo.UnitApplicationDocuments", new[] { "Unit_Id" });
            DropIndex("dbo.MemberMedicals", new[] { "Id" });
            DropForeignKey("dbo.SensitiveInfoAccesses", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.MemberEmergencyContacts", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.UnitApplicants", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.UnitApplicants", "Applicant_Id", "dbo.Members");
            DropForeignKey("dbo.UnitContacts", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.UnitApplicationDocuments", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.MemberMedicals", "Id", "dbo.Members");
            DropColumn("dbo.SarUnits", "NoApplicationsText");
            DropColumn("dbo.Members", "Status");
            DropTable("dbo.SensitiveInfoAccesses");
            DropTable("dbo.MemberEmergencyContacts");
            DropTable("dbo.UnitApplicants");
            DropTable("dbo.UnitContacts");
            DropTable("dbo.UnitApplicationDocuments");
            DropTable("dbo.MemberMedicals");
        }
    }
}
