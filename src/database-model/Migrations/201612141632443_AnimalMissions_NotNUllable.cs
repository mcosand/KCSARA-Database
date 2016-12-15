namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnimalMissions_NotNUllable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AnimalMissions", "Animal_Id", "dbo.Animals");
            DropForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters");
            DropIndex("dbo.AnimalMissions", new[] { "Animal_Id" });
            DropIndex("dbo.AnimalMissions", new[] { "MissionRoster_Id" });
            AlterColumn("dbo.AnimalMissions", "Animal_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.AnimalMissions", "MissionRoster_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.AnimalMissions", "Animal_Id");
            CreateIndex("dbo.AnimalMissions", "MissionRoster_Id");
            AddForeignKey("dbo.AnimalMissions", "Animal_Id", "dbo.Animals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters");
            DropForeignKey("dbo.AnimalMissions", "Animal_Id", "dbo.Animals");
            DropIndex("dbo.AnimalMissions", new[] { "MissionRoster_Id" });
            DropIndex("dbo.AnimalMissions", new[] { "Animal_Id" });
            AlterColumn("dbo.AnimalMissions", "MissionRoster_Id", c => c.Guid());
            AlterColumn("dbo.AnimalMissions", "Animal_Id", c => c.Guid());
            CreateIndex("dbo.AnimalMissions", "MissionRoster_Id");
            CreateIndex("dbo.AnimalMissions", "Animal_Id");
            AddForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters", "Id");
            AddForeignKey("dbo.AnimalMissions", "Animal_Id", "dbo.Animals", "Id");
        }
    }
}
