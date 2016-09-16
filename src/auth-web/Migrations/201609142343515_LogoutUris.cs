namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LogoutUris : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientLogoutUris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        Uri = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClientId, cascadeDelete: true)
                .Index(t => t.ClientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientLogoutUris", "ClientId", "dbo.Clients");
            DropIndex("dbo.ClientLogoutUris", new[] { "ClientId" });
            DropTable("dbo.ClientLogoutUris");
        }
    }
}
