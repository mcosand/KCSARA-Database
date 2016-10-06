namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenStore : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "auth.Tokens",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        TokenType = c.Short(nullable: false),
                        SubjectId = c.String(maxLength: 200),
                        ClientId = c.String(nullable: false, maxLength: 200),
                        JsonCode = c.String(nullable: false),
                        Expiry = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => new { t.Key, t.TokenType });
            
        }
        
        public override void Down()
        {
            DropTable("auth.Tokens");
        }
    }
}
