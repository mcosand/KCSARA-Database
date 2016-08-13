namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequiredTraining : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TrainingRequireds",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CourseId = c.Guid(nullable: false),
                        WacLevel = c.Int(nullable: false),
                        From = c.DateTime(nullable: false),
                        Until = c.DateTime(nullable: false),
                        GraceMonths = c.Int(nullable: false),
                        JustOnce = c.Boolean(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingCourses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingRequireds", "CourseId", "dbo.TrainingCourses");
            DropIndex("dbo.TrainingRequireds", new[] { "CourseId" });
            DropTable("dbo.TrainingRequireds");
        }
    }
}
