namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccountResetTokens : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "auth.AccountResetTokens",
                c => new
                    {
                        AccountId = c.Guid(nullable: false),
                        Token = c.String(nullable: false, maxLength: 256),
                        CreatedUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.AccountId, t.Token })
                .ForeignKey("auth.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("auth.AccountResetTokens", "AccountId", "auth.Accounts");
            DropIndex("auth.AccountResetTokens", new[] { "AccountId" });
            DropTable("auth.AccountResetTokens");
        }
    }
}
