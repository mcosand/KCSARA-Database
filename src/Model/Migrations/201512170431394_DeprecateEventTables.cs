namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeprecateEventTables : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TrainingSarUnits");
            AddPrimaryKey("dbo.TrainingSarUnits", new[] { "SarUnit_Id", "Training_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TrainingSarUnits");
            AddPrimaryKey("dbo.TrainingSarUnits", new[] { "Training_Id", "SarUnit_Id" });
        }
    }
}
