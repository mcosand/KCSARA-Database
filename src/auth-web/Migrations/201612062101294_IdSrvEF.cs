namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IdSrvEF : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "auth.Consents",
                c => new
                    {
                        Subject = c.String(nullable: false, maxLength: 200),
                        ClientId = c.String(nullable: false, maxLength: 200),
                        Scopes = c.String(nullable: false, maxLength: 2000),
                    })
                .PrimaryKey(t => new { t.Subject, t.ClientId });
            
        }
        
        public override void Down()
        {
            DropTable("auth.Consents");
        }
    }
}
