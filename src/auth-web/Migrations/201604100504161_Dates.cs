namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dates : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoginLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        ProviderId = c.String(),
                        AccountId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            AddColumn("dbo.Accounts", "Created", c => c.DateTime());
            AddColumn("dbo.Accounts", "LastLogin", c => c.DateTime());
            AddColumn("dbo.ExternalLogins", "Created", c => c.DateTime());
            AddColumn("dbo.ExternalLogins", "LastLogin", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoginLog", "AccountId", "dbo.Accounts");
            DropIndex("dbo.LoginLog", new[] { "AccountId" });
            DropColumn("dbo.ExternalLogins", "LastLogin");
            DropColumn("dbo.ExternalLogins", "Created");
            DropColumn("dbo.Accounts", "LastLogin");
            DropColumn("dbo.Accounts", "Created");
            DropTable("dbo.LoginLog");
        }
    }
}
