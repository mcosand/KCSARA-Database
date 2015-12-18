namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewEventTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventRosters",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TimeIn = c.DateTime(),
                        TimeOut = c.DateTime(),
                        Miles = c.Int(),
                        Comments = c.String(),
                        OvertimeHours = c.Int(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Event_Id = c.Guid(),
                        Person_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Event_Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .Index(t => t.Event_Id)
                .Index(t => t.Person_Id);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        County = c.String(),
                        StateNumber = c.String(),
                        CountyNumber = c.String(),
                        MissionType = c.String(),
                        StartTime = c.DateTime(nullable: false),
                        StopTime = c.DateTime(),
                        Comments = c.String(),
                        Location = c.String(),
                        ReportCompleted = c.Boolean(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Previous_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Previous_Id)
                .Index(t => t.Previous_Id);

      Sql("INSERT INTO [Events] (Id, Title, County, StateNumber, CountyNumber, MissionType, StartTime, StopTime, Comments, Location, ReportCompleted, LastChanged, ChangedBy, Discriminator, Previous_Id)"
        + " SELECT Id, Title, County, StateNumber, CountyNumber, MissionType, StartTime, StopTime, Comments, Location, ReportCompleted, LastChanged, ChangedBy, 'Mission', Previous_Id FROM Missions");
      Sql("INSERT INTO [Events] (Id, Title, County, StateNumber, StartTime, StopTime, Comments, Location, ReportCompleted, LastChanged, ChangedBy, Discriminator)"
        + " SELECT Id, Title, County, StateNumber, StartTime, StopTime, Comments, Location, 1, LastChanged, ChangedBy, 'Training' FROM Trainings");

      Sql("INSERT INTO EventRosters(Id, TimeIn, TimeOut, Miles, Comments, OvertimeHours, LastChanged, ChangedBy, Event_Id, Person_Id)"
        + " SELECT Id, TimeIn, TimeOut, Miles, Comments, OvertimeHours, LastChanged, ChangedBy, Training_Id, Person_Id FROM TrainingRosters");
      Sql("INSERT INTO EventRosters(Id, TimeIn, TimeOut, Miles, Comments, OvertimeHours, LastChanged, ChangedBy, Event_Id, Person_Id)"
        + " SELECT Id, TimeIn, TimeOut, Miles, Comments, OvertimeHours, LastChanged, ChangedBy, Mission_Id, Person_Id FROM MissionRosters");
    }

    public override void Down()
        {
            DropForeignKey("dbo.EventRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.EventRosters", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.Events", "Previous_Id", "dbo.Events");
            DropIndex("dbo.Events", new[] { "Previous_Id" });
            DropIndex("dbo.EventRosters", new[] { "Person_Id" });
            DropIndex("dbo.EventRosters", new[] { "Event_Id" });
            DropTable("dbo.Events");
            DropTable("dbo.EventRosters");
        }
    }
}
