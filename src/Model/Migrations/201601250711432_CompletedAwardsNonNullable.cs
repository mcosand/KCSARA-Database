namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompletedAwardsNonNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ComputedTrainingAwards", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.ComputedTrainingAwards", "Course_Id", "dbo.TrainingCourses");
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Course_Id" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Member_Id" });
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "Member_Id", newName: "MemberId");
            RenameColumn(table: "dbo.TrainingAwards", name: "Member_Id", newName: "MemberId");
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "Course_Id", newName: "CourseId");
            RenameColumn(table: "dbo.TrainingAwards", name: "Course_Id", newName: "CourseId");
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "Rule_Id", newName: "RuleId");
            RenameIndex(table: "dbo.ComputedTrainingAwards", name: "IX_Rule_Id", newName: "IX_RuleId");
            RenameIndex(table: "dbo.TrainingAwards", name: "IX_Member_Id", newName: "IX_MemberId");
            RenameIndex(table: "dbo.TrainingAwards", name: "IX_Course_Id", newName: "IX_CourseId");
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ComputedTrainingAwards", "CourseId", c => c.Guid(nullable: false));
            AlterColumn("dbo.ComputedTrainingAwards", "MemberId", c => c.Guid(nullable: false));
            CreateIndex("dbo.ComputedTrainingAwards", "CourseId");
            CreateIndex("dbo.ComputedTrainingAwards", "MemberId");
            AddForeignKey("dbo.ComputedTrainingAwards", "MemberId", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ComputedTrainingAwards", "CourseId", "dbo.TrainingCourses", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComputedTrainingAwards", "CourseId", "dbo.TrainingCourses");
            DropForeignKey("dbo.ComputedTrainingAwards", "MemberId", "dbo.Members");
            DropIndex("dbo.ComputedTrainingAwards", new[] { "MemberId" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "CourseId" });
            AlterColumn("dbo.ComputedTrainingAwards", "MemberId", c => c.Guid());
            AlterColumn("dbo.ComputedTrainingAwards", "CourseId", c => c.Guid());
            AlterColumn("dbo.ComputedTrainingAwards", "Completed", c => c.DateTime());
            RenameIndex(table: "dbo.TrainingAwards", name: "IX_CourseId", newName: "IX_Course_Id");
            RenameIndex(table: "dbo.TrainingAwards", name: "IX_MemberId", newName: "IX_Member_Id");
            RenameIndex(table: "dbo.ComputedTrainingAwards", name: "IX_RuleId", newName: "IX_Rule_Id");
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "RuleId", newName: "Rule_Id");
            RenameColumn(table: "dbo.TrainingAwards", name: "CourseId", newName: "Course_Id");
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "CourseId", newName: "Course_Id");
            RenameColumn(table: "dbo.TrainingAwards", name: "MemberId", newName: "Member_Id");
            RenameColumn(table: "dbo.ComputedTrainingAwards", name: "MemberId", newName: "Member_Id");
            CreateIndex("dbo.ComputedTrainingAwards", "Member_Id");
            CreateIndex("dbo.ComputedTrainingAwards", "Course_Id");
            AddForeignKey("dbo.ComputedTrainingAwards", "Course_Id", "dbo.TrainingCourses", "Id");
            AddForeignKey("dbo.ComputedTrainingAwards", "Member_Id", "dbo.Members", "Id");
        }
    }
}
