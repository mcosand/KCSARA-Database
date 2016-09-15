namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalLogins : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalLogins",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Provider = c.String(nullable: false, maxLength: 128),
                        Login = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Id, t.Provider, t.Login })
                .ForeignKey("dbo.Members", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalLogins", "Id", "dbo.Members");
            DropIndex("dbo.ExternalLogins", new[] { "Id" });
            DropTable("dbo.ExternalLogins");
        }
    }
}
