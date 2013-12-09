namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Animals",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DemSuffix = c.String(),
                        Name = c.String(),
                        Type = c.String(),
                        Comments = c.String(),
                        PhotoFile = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AnimalOwners",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsPrimary = c.Boolean(nullable: false),
                        Starting = c.DateTime(nullable: false),
                        Ending = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Animal_Id = c.Guid(),
                        Owner_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Animals", t => t.Animal_Id)
                .ForeignKey("dbo.Members", t => t.Owner_Id)
                .Index(t => t.Animal_Id)
                .Index(t => t.Owner_Id);
            
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DEM = c.String(maxLength: 50),
                        LastName = c.String(),
                        FirstName = c.String(),
                        MiddleName = c.String(),
                        InternalBirthDate = c.DateTime(),
                        InternalGender = c.String(),
                        PhotoFile = c.String(),
                        InternalWacLevel = c.Int(nullable: false),
                        WacLevelDate = c.DateTime(nullable: false),
                        Username = c.String(),
                        Comments = c.String(),
                        BackgroundDate = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MissionLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Time = c.DateTime(nullable: false),
                        Data = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Mission_Id = c.Guid(),
                        Person_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Mission_Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .Index(t => t.Mission_Id)
                .Index(t => t.Person_Id);
            
            CreateTable(
                "dbo.Missions",
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
                        Previous_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Previous_Id)
                .Index(t => t.Previous_Id);
            
            CreateTable(
                "dbo.MissionRosters",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InternalRole = c.String(),
                        TimeIn = c.DateTime(),
                        TimeOut = c.DateTime(),
                        Miles = c.Int(),
                        Comments = c.String(),
                        OvertimeHours = c.Double(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Mission_Id = c.Guid(),
                        Person_Id = c.Guid(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Mission_Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Mission_Id)
                .Index(t => t.Person_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.SarUnits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DisplayName = c.String(nullable: false),
                        LongName = c.String(),
                        County = c.String(),
                        Comments = c.String(),
                        HasOvertime = c.Boolean(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrainingCourses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DisplayName = c.String(nullable: false),
                        FullName = c.String(),
                        Categories = c.String(),
                        WacRequired = c.Int(nullable: false),
                        ShowOnCard = c.Boolean(nullable: false),
                        ValidMonths = c.Int(),
                        OfferedFrom = c.DateTime(),
                        OfferedUntil = c.DateTime(),
                        Metadata = c.String(),
                        PrerequisiteText = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.TrainingAwards",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Completed = c.DateTime(nullable: false),
                        Expiry = c.DateTime(),
                        DocPath = c.String(),
                        metadata = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Member_Id = c.Guid(nullable: false),
                        Course_Id = c.Guid(nullable: false),
                        Roster_Id = c.Guid(),
                        TrainingRule_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Member_Id, cascadeDelete: true)
                .ForeignKey("dbo.TrainingCourses", t => t.Course_Id, cascadeDelete: true)
                .ForeignKey("dbo.TrainingRosters", t => t.Roster_Id)
                .ForeignKey("dbo.TrainingRules", t => t.TrainingRule_Id)
                .Index(t => t.Member_Id)
                .Index(t => t.Course_Id)
                .Index(t => t.Roster_Id)
                .Index(t => t.TrainingRule_Id);
            
            CreateTable(
                "dbo.TrainingRosters",
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
                        Person_Id = c.Guid(),
                        Training_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .ForeignKey("dbo.Trainings", t => t.Training_Id)
                .Index(t => t.Person_Id)
                .Index(t => t.Training_Id);
            
            CreateTable(
                "dbo.Trainings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(nullable: false),
                        County = c.String(),
                        StateNumber = c.String(),
                        StartTime = c.DateTime(nullable: false),
                        StopTime = c.DateTime(),
                        Previous = c.Guid(),
                        Comments = c.String(),
                        Location = c.String(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ComputedTrainingAwards",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Expiry = c.DateTime(),
                        Completed = c.DateTime(),
                        Course_Id = c.Guid(),
                        Rule_Id = c.Guid(),
                        Member_Id = c.Guid(),
                        Roster_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrainingCourses", t => t.Course_Id)
                .ForeignKey("dbo.TrainingRules", t => t.Rule_Id)
                .ForeignKey("dbo.Members", t => t.Member_Id)
                .ForeignKey("dbo.TrainingRosters", t => t.Roster_Id)
                .Index(t => t.Course_Id)
                .Index(t => t.Rule_Id)
                .Index(t => t.Member_Id)
                .Index(t => t.Roster_Id);
            
            CreateTable(
                "dbo.TrainingRules",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RuleText = c.String(),
                        OfferedFrom = c.DateTime(),
                        OfferedUntil = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UnitMemberships",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Activated = c.DateTime(nullable: false),
                        Comments = c.String(),
                        EndTime = c.DateTime(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Person_Id = c.Guid(),
                        Unit_Id = c.Guid(),
                        Status_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .ForeignKey("dbo.UnitStatus", t => t.Status_Id)
                .Index(t => t.Person_Id)
                .Index(t => t.Unit_Id)
                .Index(t => t.Status_Id);
            
            CreateTable(
                "dbo.UnitStatus",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        StatusName = c.String(nullable: false),
                        InternalWacLevel = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        GetsAccount = c.Boolean(nullable: false),
                        WacLevel = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Unit_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SarUnits", t => t.Unit_Id)
                .Index(t => t.Unit_Id);
            
            CreateTable(
                "dbo.AnimalMissions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Animal_Id = c.Guid(),
                        MissionRoster_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Animals", t => t.Animal_Id)
                .ForeignKey("dbo.MissionRosters", t => t.MissionRoster_Id)
                .Index(t => t.Animal_Id)
                .Index(t => t.MissionRoster_Id);
            
            CreateTable(
                "dbo.MissionDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Clouds = c.String(),
                        TempLow = c.Double(),
                        TempHigh = c.Double(),
                        WindLow = c.Double(),
                        WindHigh = c.Double(),
                        Visibility = c.Double(),
                        RainType = c.Int(),
                        RainInches = c.Double(),
                        SnowType = c.Int(),
                        SnowInches = c.Double(),
                        Terrain = c.Int(),
                        GroundCoverDensity = c.Int(),
                        GroundCoverHeight = c.Int(),
                        WaterType = c.Int(),
                        TimberType = c.Int(),
                        ElevationLow = c.Int(),
                        ElevationHigh = c.Int(),
                        Tactics = c.String(),
                        CluesMethod = c.String(),
                        TerminatedReason = c.String(),
                        Debrief = c.Boolean(),
                        Cisd = c.Boolean(),
                        Comments = c.String(),
                        EquipmentNotes = c.String(),
                        Topography = c.Int(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Person_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .ForeignKey("dbo.Missions", t => t.Id)
                .Index(t => t.Person_Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SubjectGroups",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Number = c.Int(nullable: false),
                        WhenLost = c.DateTime(),
                        WhenReported = c.DateTime(),
                        WhenCalled = c.DateTime(),
                        WhenAtPls = c.DateTime(),
                        WhenFound = c.DateTime(),
                        PlsEasting = c.String(),
                        PlsNorthing = c.String(),
                        PlsCertainty = c.Int(),
                        PlsElevation = c.Int(),
                        PlsCommonName = c.String(),
                        FoundEasting = c.String(),
                        FoundNorthing = c.String(),
                        FoundCertainty = c.Int(),
                        FoundCondition = c.String(),
                        FoundElevation = c.Int(),
                        FoundTactics = c.String(),
                        Category = c.String(),
                        Cause = c.String(),
                        Behavior = c.String(),
                        Comments = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Mission_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Mission_Id)
                .Index(t => t.Mission_Id);
            
            CreateTable(
                "dbo.SubjectGroupLinks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Number = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Subject_Id = c.Guid(),
                        Group_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subjects", t => t.Subject_Id)
                .ForeignKey("dbo.SubjectGroups", t => t.Group_Id)
                .Index(t => t.Subject_Id)
                .Index(t => t.Group_Id);
            
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        InternalGender = c.String(),
                        BirthYear = c.Int(),
                        Address = c.String(),
                        HomePhone = c.String(),
                        WorkPhone = c.String(),
                        OtherPhone = c.String(),
                        Comments = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MissionGeographies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InstanceId = c.Guid(),
                        Kind = c.String(),
                        Time = c.DateTime(),
                        Description = c.String(),
                        LocationBinary = c.String(),
                        LocationText = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Mission_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Missions", t => t.Mission_Id)
                .Index(t => t.Mission_Id);
            
            CreateTable(
                "dbo.PersonAddresses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InternalType = c.String(),
                        Street = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Zip = c.String(),
                        Geo = c.String(),
                        Quality = c.Int(nullable: false),
                        RosterVisibility = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Person_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .Index(t => t.Person_Id);
            
            CreateTable(
                "dbo.PersonContacts",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        Subtype = c.String(),
                        Value = c.String(),
                        Priority = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                        Person_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.Person_Id)
                .Index(t => t.Person_Id);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReferenceId = c.Guid(nullable: false),
                        Type = c.String(),
                        FileName = c.String(),
                        MimeType = c.String(),
                        Size = c.Int(nullable: false),
                        StorePath = c.String(),
                        Description = c.String(),
                        LastChanged = c.DateTime(nullable: false),
                        ChangedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrainingExpirationSummaries",
                c => new
                    {
                        CourseId = c.Guid(nullable: false),
                        Expired = c.Int(nullable: false),
                        Recent = c.Int(nullable: false),
                        Almost = c.Int(nullable: false),
                        Good = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CourseId);
            
            CreateTable(
                "dbo.CurrentMemberIds",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.xref_county_id",
                c => new
                    {
                        accessMemberID = c.Int(nullable: false),
                        personId = c.Guid(nullable: false),
                        ExternalSource = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.accessMemberID, t.personId, t.ExternalSource });
            
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ObjectId = c.Guid(nullable: false),
                        Action = c.String(),
                        Comment = c.String(),
                        User = c.String(),
                        Changed = c.DateTime(nullable: false),
                        Collection = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrainingTrainingCourses",
                c => new
                    {
                        Training_Id = c.Guid(nullable: false),
                        TrainingCourse_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Training_Id, t.TrainingCourse_Id })
                .ForeignKey("dbo.Trainings", t => t.Training_Id, cascadeDelete: true)
                .ForeignKey("dbo.TrainingCourses", t => t.TrainingCourse_Id, cascadeDelete: true)
                .Index(t => t.Training_Id)
                .Index(t => t.TrainingCourse_Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.TrainingTrainingCourses", new[] { "TrainingCourse_Id" });
            DropIndex("dbo.TrainingTrainingCourses", new[] { "Training_Id" });
            DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
            DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
            DropIndex("dbo.MissionGeographies", new[] { "Mission_Id" });
            DropIndex("dbo.SubjectGroupLinks", new[] { "Group_Id" });
            DropIndex("dbo.SubjectGroupLinks", new[] { "Subject_Id" });
            DropIndex("dbo.SubjectGroups", new[] { "Mission_Id" });
            DropIndex("dbo.MissionDetails", new[] { "Id" });
            DropIndex("dbo.MissionDetails", new[] { "Person_Id" });
            DropIndex("dbo.AnimalMissions", new[] { "MissionRoster_Id" });
            DropIndex("dbo.AnimalMissions", new[] { "Animal_Id" });
            DropIndex("dbo.UnitStatus", new[] { "Unit_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Status_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Unit_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Roster_Id" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Member_Id" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Rule_Id" });
            DropIndex("dbo.ComputedTrainingAwards", new[] { "Course_Id" });
            DropIndex("dbo.TrainingRosters", new[] { "Training_Id" });
            DropIndex("dbo.TrainingRosters", new[] { "Person_Id" });
            DropIndex("dbo.TrainingAwards", new[] { "TrainingRule_Id" });
            DropIndex("dbo.TrainingAwards", new[] { "Roster_Id" });
            DropIndex("dbo.TrainingAwards", new[] { "Course_Id" });
            DropIndex("dbo.TrainingAwards", new[] { "Member_Id" });
            DropIndex("dbo.TrainingCourses", new[] { "Unit_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Unit_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Person_Id" });
            DropIndex("dbo.MissionRosters", new[] { "Mission_Id" });
            DropIndex("dbo.Missions", new[] { "Previous_Id" });
            DropIndex("dbo.MissionLogs", new[] { "Person_Id" });
            DropIndex("dbo.MissionLogs", new[] { "Mission_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
            DropForeignKey("dbo.TrainingTrainingCourses", "TrainingCourse_Id", "dbo.TrainingCourses");
            DropForeignKey("dbo.TrainingTrainingCourses", "Training_Id", "dbo.Trainings");
            DropForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionGeographies", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.SubjectGroupLinks", "Group_Id", "dbo.SubjectGroups");
            DropForeignKey("dbo.SubjectGroupLinks", "Subject_Id", "dbo.Subjects");
            DropForeignKey("dbo.SubjectGroups", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.MissionDetails", "Id", "dbo.Missions");
            DropForeignKey("dbo.MissionDetails", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.AnimalMissions", "MissionRoster_Id", "dbo.MissionRosters");
            DropForeignKey("dbo.AnimalMissions", "Animal_Id", "dbo.Animals");
            DropForeignKey("dbo.UnitStatus", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.UnitMemberships", "Status_Id", "dbo.UnitStatus");
            DropForeignKey("dbo.UnitMemberships", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.ComputedTrainingAwards", "Roster_Id", "dbo.TrainingRosters");
            DropForeignKey("dbo.ComputedTrainingAwards", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.ComputedTrainingAwards", "Rule_Id", "dbo.TrainingRules");
            DropForeignKey("dbo.ComputedTrainingAwards", "Course_Id", "dbo.TrainingCourses");
            DropForeignKey("dbo.TrainingRosters", "Training_Id", "dbo.Trainings");
            DropForeignKey("dbo.TrainingRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.TrainingAwards", "TrainingRule_Id", "dbo.TrainingRules");
            DropForeignKey("dbo.TrainingAwards", "Roster_Id", "dbo.TrainingRosters");
            DropForeignKey("dbo.TrainingAwards", "Course_Id", "dbo.TrainingCourses");
            DropForeignKey("dbo.TrainingAwards", "Member_Id", "dbo.Members");
            DropForeignKey("dbo.TrainingCourses", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.MissionRosters", "Unit_Id", "dbo.SarUnits");
            DropForeignKey("dbo.MissionRosters", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionRosters", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.Missions", "Previous_Id", "dbo.Missions");
            DropForeignKey("dbo.MissionLogs", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals");
            DropTable("dbo.TrainingTrainingCourses");
            DropTable("dbo.AuditLogs");
            DropTable("dbo.xref_county_id");
            DropTable("dbo.CurrentMemberIds");
            DropTable("dbo.TrainingExpirationSummaries");
            DropTable("dbo.Documents");
            DropTable("dbo.PersonContacts");
            DropTable("dbo.PersonAddresses");
            DropTable("dbo.MissionGeographies");
            DropTable("dbo.Subjects");
            DropTable("dbo.SubjectGroupLinks");
            DropTable("dbo.SubjectGroups");
            DropTable("dbo.MissionDetails");
            DropTable("dbo.AnimalMissions");
            DropTable("dbo.UnitStatus");
            DropTable("dbo.UnitMemberships");
            DropTable("dbo.TrainingRules");
            DropTable("dbo.ComputedTrainingAwards");
            DropTable("dbo.Trainings");
            DropTable("dbo.TrainingRosters");
            DropTable("dbo.TrainingAwards");
            DropTable("dbo.TrainingCourses");
            DropTable("dbo.SarUnits");
            DropTable("dbo.MissionRosters");
            DropTable("dbo.Missions");
            DropTable("dbo.MissionLogs");
            DropTable("dbo.Members");
            DropTable("dbo.AnimalOwners");
            DropTable("dbo.Animals");
        }
    }
}
