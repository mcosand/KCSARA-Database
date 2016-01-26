namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompletedAwardsNonNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTime());
        }
    }
}
