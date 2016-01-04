namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class EventLogs : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions");
      RenameTable(name: "dbo.MissionLogs", newName: "EventLogs");
      DropIndex("dbo.EventLogs", new[] { "Mission_Id" });
      RenameColumn(table: "dbo.EventLogs", name: "Person_Id", newName: "PersonId");
      RenameIndex(table: "dbo.EventLogs", name: "IX_Person_Id", newName: "IX_PersonId");
      AddColumn("dbo.EventLogs", "EventId", c => c.Guid(nullable: false));
      CreateIndex("dbo.EventLogs", "EventId");

      Sql("UPDATE dbo.EventLogs SET EventId = Mission_Id");

      AddForeignKey("dbo.EventLogs", "EventId", "dbo.Events", "Id", cascadeDelete: true);
      DropColumn("dbo.EventLogs", "Mission_Id");
    }

    public override void Down()
    {
      AddColumn("dbo.EventLogs", "Mission_Id", c => c.Guid(nullable: false));

      Sql("UPDATE dbo.EventLogs SET Mission_Id = EventId");

      DropForeignKey("dbo.EventLogs", "EventId", "dbo.Events");
      DropIndex("dbo.EventLogs", new[] { "EventId" });
      DropColumn("dbo.EventLogs", "EventId");
      RenameIndex(table: "dbo.EventLogs", name: "IX_PersonId", newName: "IX_Person_Id");
      RenameColumn(table: "dbo.EventLogs", name: "PersonId", newName: "Person_Id");
      CreateIndex("dbo.EventLogs", "Mission_Id");
      RenameTable(name: "dbo.EventLogs", newName: "MissionLogs");
      AddForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions", "Id", cascadeDelete: true);
    }
  }
}
