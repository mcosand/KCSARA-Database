namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FlowType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "Flow", c => c.Int(nullable: false, defaultValue: (int)IdentityServer3.Core.Models.Flows.Hybrid));
            DropColumn("dbo.Clients", "UseClientCredentialFlow");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Clients", "UseClientCredentialFlow", c => c.Boolean(nullable: false, defaultValue: false));
            DropColumn("dbo.Clients", "Flow");
        }
    }
}
