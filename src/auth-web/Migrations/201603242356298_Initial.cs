namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Username = c.String(maxLength: 100),
                        PasswordHash = c.String(maxLength: 500),
                        FirstName = c.String(maxLength: 100),
                        LastName = c.String(maxLength: 100),
                        Email = c.String(maxLength: 100),
                        MemberId = c.Guid(),
                        LockReason = c.String(maxLength: 255),
                        Locked = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.String(nullable: false, maxLength: 50),
                        DisplayName = c.String(nullable: false, maxLength: 80),
                        Enabled = c.Boolean(nullable: false),
                        AddedScopes = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ClientId, unique: true);
            
            CreateTable(
                "dbo.ClientUris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Uri = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
            CreateTable(
                "dbo.ExternalLogins",
                c => new
                    {
                        Provider = c.String(nullable: false, maxLength: 50),
                        ProviderId = c.String(nullable: false, maxLength: 255),
                        AccountId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Provider, t.ProviderId })
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.Verifications",
                c => new
                    {
                        Provider = c.String(nullable: false, maxLength: 50),
                        ProviderId = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 100),
                        Code = c.String(nullable: false, maxLength: 50),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.Provider, t.ProviderId });
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalLogins", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.ClientUris", "ClientId", "dbo.Clients");
            DropIndex("dbo.ExternalLogins", new[] { "AccountId" });
            DropIndex("dbo.ClientUris", new[] { "ClientId" });
            DropIndex("dbo.Clients", new[] { "ClientId" });
            DropTable("dbo.Verifications");
            DropTable("dbo.ExternalLogins");
            DropTable("dbo.ClientUris");
            DropTable("dbo.Clients");
            DropTable("dbo.Accounts");
        }
    }
}
