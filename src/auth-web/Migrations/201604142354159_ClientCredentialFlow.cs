namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClientCredentialFlow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "UseClientCredentialFlow", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "UseClientCredentialFlow");
        }
    }
}
