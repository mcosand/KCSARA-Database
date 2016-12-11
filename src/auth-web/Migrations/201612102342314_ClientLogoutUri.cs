namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClientLogoutUri : DbMigration
    {
        public override void Up()
        {
            AddColumn("auth.Clients", "LogoutUri", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("auth.Clients", "LogoutUri");
        }
    }
}
