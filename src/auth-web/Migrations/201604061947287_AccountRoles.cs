namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccountRoles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 25),
                        Name = c.String(maxLength: 100),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AccountRoles",
                c => new
                    {
                        RoleRow_Id = c.String(nullable: false, maxLength: 25),
                        AccountRow_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RoleRow_Id, t.AccountRow_Id })
                .ForeignKey("dbo.Roles", t => t.RoleRow_Id, cascadeDelete: true)
                .ForeignKey("dbo.Accounts", t => t.AccountRow_Id, cascadeDelete: true)
                .Index(t => t.RoleRow_Id)
                .Index(t => t.AccountRow_Id);
            
            CreateTable(
                "dbo.RoleRoles",
                c => new
                    {
                        RoleRow_Id = c.String(nullable: false, maxLength: 25),
                        RoleRow_Id1 = c.String(nullable: false, maxLength: 25),
                    })
                .PrimaryKey(t => new { t.RoleRow_Id, t.RoleRow_Id1 })
                .ForeignKey("dbo.Roles", t => t.RoleRow_Id)
                .ForeignKey("dbo.Roles", t => t.RoleRow_Id1)
                .Index(t => t.RoleRow_Id)
                .Index(t => t.RoleRow_Id1);
            
            CreateTable(
                "dbo.RoleOwners",
                c => new
                    {
                        RoleRow_Id = c.String(nullable: false, maxLength: 25),
                        AccountRow_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.RoleRow_Id, t.AccountRow_Id })
                .ForeignKey("dbo.Roles", t => t.RoleRow_Id, cascadeDelete: true)
                .ForeignKey("dbo.Accounts", t => t.AccountRow_Id, cascadeDelete: true)
                .Index(t => t.RoleRow_Id)
                .Index(t => t.AccountRow_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoleOwners", "AccountRow_Id", "dbo.Accounts");
            DropForeignKey("dbo.RoleOwners", "RoleRow_Id", "dbo.Roles");
            DropForeignKey("dbo.RoleRoles", "RoleRow_Id1", "dbo.Roles");
            DropForeignKey("dbo.RoleRoles", "RoleRow_Id", "dbo.Roles");
            DropForeignKey("dbo.AccountRoles", "AccountRow_Id", "dbo.Accounts");
            DropForeignKey("dbo.AccountRoles", "RoleRow_Id", "dbo.Roles");
            DropIndex("dbo.RoleOwners", new[] { "AccountRow_Id" });
            DropIndex("dbo.RoleOwners", new[] { "RoleRow_Id" });
            DropIndex("dbo.RoleRoles", new[] { "RoleRow_Id1" });
            DropIndex("dbo.RoleRoles", new[] { "RoleRow_Id" });
            DropIndex("dbo.AccountRoles", new[] { "AccountRow_Id" });
            DropIndex("dbo.AccountRoles", new[] { "RoleRow_Id" });
            DropTable("dbo.RoleOwners");
            DropTable("dbo.RoleRoles");
            DropTable("dbo.AccountRoles");
            DropTable("dbo.Roles");
        }
    }
}
