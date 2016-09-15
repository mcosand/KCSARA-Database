namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dates2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "PasswordDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Accounts", "PasswordDate");
        }
    }
}
