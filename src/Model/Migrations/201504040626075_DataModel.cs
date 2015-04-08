namespace Kcsar.Database.Data.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class DataModel : DbMigration
  {
    public override void Up()
    {
      DropTable("dbo.TrainingExpirationSummaries");
      DropTable("dbo.CurrentMemberIds");

      DropForeignKey("dbo.AnimalEvents", "FK_dbo.AnimalMissions_dbo.Animals_Animal_Id");
      DropIndex("dbo.AnimalEvents", new[] { "Animal_Id" });
      DropIndex("dbo.AnimalEvents", new[] { "RosterId" });
      DropForeignKey("dbo.AnimalOwners", "FK_dbo.AnimalOwners_dbo.Animals_Animal_Id");
      DropForeignKey("dbo.AnimalOwners", "FK_dbo.AnimalOwners_dbo.Members_Owner_Id");
      DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
      DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
      DropForeignKey("dbo.ComputedTrainingAwards", "FK_dbo.ComputedTrainingAwards_dbo.Members_Member_Id");
      DropForeignKey("dbo.ComputedTrainingAwards", "FK_dbo.ComputedTrainingAwards_dbo.Participants_AttendanceId");
      DropForeignKey("dbo.ComputedTrainingAwards", "FK_dbo.ComputedTrainingAwards_dbo.TrainingCourses_Course_Id");
      DropForeignKey("dbo.ComputedTrainingAwards", "FK_dbo.ComputedTrainingAwards_dbo.TrainingRules_Rule_Id");
      DropIndex("dbo.ComputedTrainingAwards", new[] {"Course_Id"});
      DropIndex("dbo.ComputedTrainingAwards", new[] {"Member_Id"});
      DropIndex("dbo.ComputedTrainingAwards", new[] {"Rule_Id"});
      DropForeignKey("dbo.EventDetails", "FK_dbo.MissionDetails_dbo.Members_Person_Id");
      DropForeignKey("dbo.EventDetails", "FK_dbo.MissionDetails_dbo.SarEvents_Id");
      DropIndex("dbo.EventDetails", new[] { "Person_Id" });
      DropForeignKey("dbo.EventGeographies", "FK_dbo.MissionGeographies_dbo.SarEvents_EventId");
      DropForeignKey("dbo.PersonAddresses", "FK_dbo.PersonAddresses_dbo.Members_Person_Id");
      DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
      DropForeignKey("dbo.PersonContacts", "FK_dbo.PersonContacts_dbo.Members_Person_Id");
      DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
      DropForeignKey("dbo.MemberEmergencyContacts", "FK_dbo.MemberEmergencyContacts_dbo.Members_Member_Id");
      DropIndex("dbo.MemberEmergencyContacts", "IX_Member_Id");
      DropForeignKey("dbo.MemberUnitDocuments", "FK_dbo.MemberUnitDocuments_dbo.Members_Member_Id");
      DropForeignKey("dbo.MemberUnitDocuments", "FK_dbo.MemberUnitDocuments_dbo.UnitDocuments_Document_Id");
      DropIndex("dbo.MemberUnitDocuments", new[] { "Member_Id" });
      DropIndex("dbo.MemberUnitDocuments", new[] { "Document_Id" });
      DropForeignKey("dbo.ParticipatingUnits", "FK_dbo.ParticipatingUnits_dbo.SarUnits_MemberUnitId");
      DropForeignKey("dbo.SarEvents", "FK_dbo.SarEvents_dbo.SarEvents_Previous_Id");
      DropIndex("dbo.SarEvents", new[] { "Previous_Id" });
      DropForeignKey("dbo.SubjectGroupLinks", "FK_dbo.SubjectGroupLinks_dbo.SubjectGroups_Group_Id");
      DropForeignKey("dbo.SubjectGroupLinks", "FK_dbo.SubjectGroupLinks_dbo.Subjects_Subject_Id");
      DropIndex("dbo.SubjectGroupLinks", new[] { "Group_Id" });
      DropIndex("dbo.SubjectGroupLinks", new[] { "Subject_Id" });
      DropForeignKey("dbo.SensitiveInfoAccesses", "FK_dbo.SensitiveInfoAccesses_dbo.Members_Owner_Id");
      DropIndex("dbo.SensitiveInfoAccesses", new[] { "Owner_Id" });
      DropForeignKey("dbo.TrainingCourses", "FK_dbo.TrainingCourses_dbo.SarUnits_Unit_Id");
      DropIndex("dbo.TrainingCourses", new[] { "Unit_Id" });
      DropForeignKey("dbo.TrainingAwards", "FK_dbo.TrainingAwards_dbo.Members_Member_Id");
      DropForeignKey("dbo.TrainingAwards", "FK_dbo.TrainingAwards_dbo.Participants_AttendanceId");
      DropForeignKey("dbo.TrainingAwards", "FK_dbo.TrainingAwards_dbo.TrainingCourses_Course_Id");
      DropForeignKey("dbo.TrainingAwards", "FK_dbo.TrainingAwards_dbo.TrainingRules_TrainingRule_Id");
      DropIndex("dbo.TrainingAwards", new[] { "Course_Id" });
      DropIndex("dbo.TrainingAwards", new[] { "Member_Id" });
      DropIndex("dbo.TrainingAwards", new[] { "TrainingRule_Id" });
      DropIndex("dbo.TrainingTrainingCourses", new[] { "Training_Id" });
      DropIndex("dbo.TrainingTrainingCourses", new[] { "TrainingCourse_Id" });
      DropForeignKey("dbo.TrainingTrainingCourses", "FK_dbo.Training2TrainingCourse_dbo.TrainingCourses_TrainingCourse_Id");
      DropForeignKey("dbo.TrainingTrainingCourses", "FK_dbo.TrainingTrainingCourses_dbo.SarEvents_Training_Id");
      DropIndex("dbo.UnitApplicants", new[] { "Applicant_Id" });
      DropIndex("dbo.UnitApplicants", new[] { "Unit_Id" });
      DropForeignKey("dbo.UnitApplicants", "FK_dbo.UnitApplicants_dbo.Members_Applicant_Id");
      DropForeignKey("dbo.UnitApplicants", "FK_dbo.UnitApplicants_dbo.SarUnits_Unit_Id");
      DropIndex("dbo.UnitContacts", new[] { "Unit_Id" });
      DropForeignKey("dbo.UnitContacts", "FK_dbo.UnitContacts_dbo.SarUnits_Unit_Id");
      DropIndex("dbo.UnitDocuments", new[] { "Unit_Id" });
      DropForeignKey("dbo.UnitDocuments", "FK_dbo.UnitDocuments_dbo.SarUnits_Unit_Id");
      DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
      DropIndex("dbo.UnitMemberships", new[] { "Unit_Id" });
      DropIndex("dbo.UnitMemberships", new[] { "Status_Id" });
      DropForeignKey("dbo.UnitMemberships", "FK_dbo.UnitMemberships_dbo.Members_Person_Id");
      DropForeignKey("dbo.UnitMemberships", "FK_dbo.UnitMemberships_dbo.SarUnits_Unit_Id");
      DropForeignKey("dbo.UnitMemberships", "FK_dbo.UnitMemberships_dbo.UnitStatus_Status_Id");
      DropIndex("dbo.UnitStatus", new[] { "Unit_Id" });
      DropForeignKey("dbo.UnitStatus", "FK_dbo.UnitStatus_dbo.SarUnits_Unit_Id");

      RenameTable("dbo.PersonAddresses", "MemberAddresses");
      RenameTable("dbo.SarUnits", "Units");
      RenameTable("dbo.TrainingAwards", "TrainingRecords");
      RenameTable("dbo.ComputedTrainingAwards", "ComputedTrainingRecords");
      RenameTable("dbo.PersonContacts", "MemberContacts");
      RenameTable("dbo.TrainingTrainingCourses", "TrainingRowTrainingCourseRows");
      
      RenameColumn("dbo.AnimalEvents", "Animal_Id", "AnimalId");
      AlterColumn("dbo.AnimalEvents", "AnimalId", c => c.Guid(nullable: false));
      AlterColumn("dbo.AnimalEvents", "RosterId", c => c.Guid(nullable: false));
      DropColumn("dbo.AnimalEvents", "MissionRoster_Id");
      AddForeignKey("dbo.AnimalEvents", "AnimalId", "dbo.Animals", cascadeDelete: true);
      Console.WriteLine("Wait for timing glitch?"); // When this statement is not here, upgrades can't find Animalevents.AnimalId
      CreateIndex("dbo.AnimalEvents", "AnimalId");
      CreateIndex("dbo.AnimalEvents", "RosterId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.AnimalMissions]', 'PK_dbo.AnimalEvents'");
      RenameColumn("dbo.AnimalOwners", "Animal_Id", "AnimalId");
      RenameColumn("dbo.AnimalOwners", "Owner_Id", "OwnerId");
      CreateIndex("dbo.AnimalOwners", "AnimalId");
      CreateIndex("dbo.AnimalOwners", "OwnerId");
      AddForeignKey("dbo.AnimalOwners", "AnimalId", "dbo.Animals", cascadeDelete: true);
      AddForeignKey("dbo.AnimalOwners", "OwnerId", "dbo.Members", cascadeDelete: true);

      RenameColumn("dbo.ComputedTrainingRecords", "Course_Id", "CourseId");
      RenameColumn("dbo.ComputedTrainingRecords", "Rule_Id", "RuleId");
      RenameColumn("dbo.ComputedTrainingRecords", "Member_Id", "MemberId");
      AlterColumn("dbo.ComputedTrainingRecords", "CourseId", c => c.Guid(nullable: false));
      AlterColumn("dbo.ComputedTrainingRecords", "MemberId", c => c.Guid(nullable: false));
      CreateIndex("dbo.ComputedTrainingRecords", "CourseId");
      CreateIndex("dbo.ComputedTrainingRecords", "MemberId");
      CreateIndex("dbo.ComputedTrainingRecords", "RuleId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.ComputedTrainingAwards]', 'PK_dbo.ComputedTrainingRecords'");
      AddForeignKey("dbo.ComputedTrainingRecords", "MemberId", "dbo.Members", cascadeDelete: true);
      AddForeignKey("dbo.ComputedTrainingRecords", "AttendanceId", "dbo.Participants");
      AddForeignKey("dbo.ComputedTrainingRecords", "CourseId", "dbo.TrainingCourses", cascadeDelete: true);
      AddForeignKey("dbo.ComputedTrainingRecords", "RuleId", "dbo.TrainingRules");

      RenameColumn("dbo.EventDetails", "Person_Id", "MemberId");
      CreateIndex("dbo.EventDetails", "MemberId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.MissionDetails]', 'PK_dbo.EventDetails'");
      AddForeignKey("dbo.EventDetails", "MemberId", "dbo.Members");
      AddForeignKey("dbo.EventDetails", "Id", "dbo.SarEvents", cascadeDelete: true);
      Sql(@"EXECUTE sp_rename '[PK_dbo.MissionGeographies]', 'PK_dbo.EventGeographies'");
      AddForeignKey("dbo.EventGeographies", "EventId", "dbo.SarEvents", cascadeDelete: true);

      Sql(@"EXECUTE sp_rename '[PK_dbo.PersonAddresses]', 'PK_dbo.MemberAddresses'");
      RenameColumn("dbo.MemberAddresses", "Person_Id", "MemberId");
      AddForeignKey("dbo.MemberAddresses", "MemberId", "dbo.Members", cascadeDelete: true);
      CreateIndex("dbo.MemberAddresses", "MemberId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.PersonContacts]', 'PK_dbo.MemberContacts'");
      RenameColumn("dbo.MemberContacts", "Person_Id", "MemberId");
      AddForeignKey("dbo.MemberContacts", "MemberId", "dbo.Members", cascadeDelete: true);
      CreateIndex("dbo.MemberContacts", "MemberId");
      RenameColumn("dbo.MemberEmergencyContacts", "Member_Id", "MemberId");
      AlterColumn("dbo.MemberEmergencyContacts", "MemberId", c => c.Guid(nullable: false));
      AddForeignKey("dbo.MemberEmergencyContacts", "MemberId", "dbo.Members", cascadeDelete: true);
      CreateIndex("dbo.MemberEmergencyContacts", "MemberId");
      Sql(@"IF NOT EXISTS (
  SELECT 1 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Members]') 
         AND name = 'SheriffApp'
) ALTER TABLE Members ADD SheriffApp DATETIME NULL");

      RenameColumn("dbo.MemberUnitDocuments", "Member_Id", "MemberId");
      RenameColumn("dbo.MemberUnitDocuments", "Document_Id", "DocumentId");
      AddForeignKey("dbo.MemberUnitDocuments", "MemberId", "dbo.Members", cascadeDelete: true);
      AddForeignKey("dbo.MemberUnitDocuments", "DocumentId", "dbo.UnitDocuments", cascadeDelete: true);
      CreateIndex("dbo.MemberUnitDocuments", "MemberId");
      CreateIndex("dbo.MemberUnitDocuments", "DocumentId");

      AddForeignKey("dbo.ParticipatingUnits", "MemberUnitId", "dbo.Units");

      RenameColumn("dbo.SarEvents", "Previous_Id", "PreviousId");
      AddForeignKey("dbo.SarEvents", "PreviousId", "dbo.SarEvents");
      CreateIndex("dbo.SarEvents", "PreviousId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.SarUnits]', 'PK_dbo.Units'");
      RenameColumn("dbo.SubjectGroupLinks", "Subject_Id", "SubjectId");
      RenameColumn("dbo.SubjectGroupLinks", "Group_Id", "GroupId");
      AlterColumn("dbo.SubjectGroupLinks", "SubjectId", c => c.Guid(nullable: false));
      AlterColumn("dbo.SubjectGroupLinks", "GroupId", c => c.Guid(nullable: false));
      CreateIndex("dbo.SubjectGroupLinks", "GroupId");
      CreateIndex("dbo.SubjectGroupLinks", "SubjectId");
      AddForeignKey("dbo.SubjectGroupLinks", "GroupId", "dbo.SubjectGroups", cascadeDelete: true);
      AddForeignKey("dbo.SubjectGroupLinks", "SubjectId", "dbo.Subjects", cascadeDelete: true);

      RenameColumn("dbo.SensitiveInfoAccesses", "Owner_Id", "OwnerId");
      AlterColumn("dbo.SensitiveInfoAccesses", "OwnerId", c => c.Guid(nullable: false));
      CreateIndex("dbo.SensitiveInfoAccesses", "OwnerId");
      AddForeignKey("dbo.SensitiveInfoAccesses", "OwnerId", "dbo.Members", cascadeDelete: true);

      RenameColumn("dbo.TrainingCourses", "Unit_Id", "UnitId");
      CreateIndex("dbo.TrainingCourses", "UnitId");
      AddForeignKey("dbo.TrainingCourses", "UnitId", "dbo.Units");
      DropColumn("dbo.TrainingRecords", "TrainingRule_Id");
      RenameColumn("dbo.TrainingRecords", "Member_Id", "MemberId");
      RenameColumn("dbo.TrainingRecords", "Course_Id", "CourseId");
      Sql(@"EXECUTE sp_rename '[PK_dbo.TrainingAwards]', 'PK_dbo.TrainingRecords'");
      CreateIndex("dbo.TrainingRecords", "MemberId");
      CreateIndex("dbo.TrainingRecords", "CourseId");
      AddForeignKey("dbo.TrainingRecords", "CourseId", "dbo.TrainingCourses", cascadeDelete: true);
      AddForeignKey("dbo.TrainingRecords", "MemberId", "dbo.Members", cascadeDelete: true);
      AddForeignKey("dbo.TrainingRecords", "AttendanceId", "dbo.Participants");

      RenameColumn("dbo.TrainingRowTrainingCourseRows", "Training_Id", "TrainingRow_Id");
      RenameColumn("dbo.TrainingRowTrainingCourseRows", "TrainingCourse_Id", "TrainingCourseRow_Id");
      Sql(@"EXECUTE sp_rename '[PK_dbo.Training2TrainingCourse]', 'PK_dbo.TrainingRowTrainingCourseRows'");
      CreateIndex("dbo.TrainingRowTrainingCourseRows", "TrainingRow_Id");
      CreateIndex("dbo.TrainingRowTrainingCourseRows", "TrainingCourseRow_Id");
      AddForeignKey("dbo.TrainingRowTrainingCourseRows", "TrainingRow_Id", "dbo.SarEvents", cascadeDelete: true);
      AddForeignKey("dbo.TrainingRowTrainingCourseRows", "TrainingCourseRow_Id", "dbo.TrainingCourses", cascadeDelete: true);

      RenameColumn("dbo.UnitApplicants", "Unit_Id", "UnitId");
      RenameColumn("dbo.UnitApplicants", "Applicant_Id", "ApplicantId");
      AlterColumn("dbo.UnitApplicants", "UnitId", c => c.Guid(nullable: false));
      AlterColumn("dbo.UnitApplicants", "ApplicantId", c => c.Guid(nullable: false));
      CreateIndex("dbo.UnitApplicants", "ApplicantId");
      CreateIndex("dbo.UnitApplicants", "UnitId");
      AddForeignKey("dbo.UnitApplicants", "ApplicantId", "dbo.Members", cascadeDelete: true);
      AddForeignKey("dbo.UnitApplicants", "UnitId", "dbo.Units", cascadeDelete: true);
      RenameColumn("dbo.UnitContacts", "Unit_Id", "UnitId");
      AlterColumn("dbo.UnitContacts", "UnitId", c => c.Guid(nullable: false));
      CreateIndex("dbo.UnitContacts", "UnitId");
      AddForeignKey("dbo.UnitContacts", "UnitId", "dbo.Units", cascadeDelete: true);
      RenameColumn("dbo.UnitDocuments", "Unit_Id", "UnitId");
      AlterColumn("dbo.UnitDocuments", "UnitId", c => c.Guid(nullable: false));
      CreateIndex("dbo.UnitDocuments", "UnitId");
      AddForeignKey("dbo.UnitDocuments", "UnitId", "dbo.Units", cascadeDelete: true);
      RenameColumn("dbo.UnitMemberships", "Person_Id", "MemberId");
      RenameColumn("dbo.UnitMemberships", "Unit_Id", "UnitId");
      RenameColumn("dbo.UnitMemberships", "Status_Id", "StatusId");
      AlterColumn("dbo.UnitMemberships", "UnitId", c => c.Guid(nullable: false));
      AlterColumn("dbo.UnitMemberships", "StatusId", c => c.Guid(nullable: false));
      CreateIndex("dbo.UnitMemberships", "MemberId");
      CreateIndex("dbo.UnitMemberships", "UnitId");
      CreateIndex("dbo.UnitMemberships", "StatusId");
      AddForeignKey("dbo.UnitMemberships", "MemberId", "dbo.Members", cascadeDelete: true);
      AddForeignKey("dbo.UnitMemberships", "UnitId", "dbo.Units", cascadeDelete: true);
      AddForeignKey("dbo.UnitMemberships", "StatusId", "dbo.UnitStatus");
      RenameColumn("dbo.UnitStatus", "Unit_Id", "UnitId");
      AlterColumn("dbo.UnitStatus", "UnitId", c => c.Guid(nullable: false));
      DropColumn("dbo.UnitStatus", "InternalWacLevel");
      CreateIndex("dbo.UnitStatus", "UnitId");
      AddForeignKey("dbo.UnitStatus", "UnitId", "dbo.Units", cascadeDelete: true);
    }

    public override void Down()
    {
      throw new NotImplementedException("no going back");
    }
  }
}
