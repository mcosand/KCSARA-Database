namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EventLogsKeys : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.EventLogs");
            AddPrimaryKey("dbo.EventLogs", new[] { "EventId", "Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.EventLogs");
            AddPrimaryKey("dbo.EventLogs", "Id");
        }
    }
}
