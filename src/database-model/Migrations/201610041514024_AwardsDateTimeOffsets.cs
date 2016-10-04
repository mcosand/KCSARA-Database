namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AwardsDateTimeOffsets : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ComputedTrainingAwards", "Expiry", c => c.DateTimeOffset(precision: 7));
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTimeOffset(precision: 7));
            AlterColumn("dbo.TrainingAwards", "Completed", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.TrainingAwards", "Expiry", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TrainingAwards", "Expiry", c => c.DateTime());
            AlterColumn("dbo.TrainingAwards", "Completed", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTime());
            AlterColumn("dbo.ComputedTrainingAwards", "Expiry", c => c.DateTime());
        }
    }
}
