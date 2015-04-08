namespace Kcsar.Database.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorValidate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals");
            DropForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.MissionDetails", "Id", "dbo.Missions");
            DropForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members");
            DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
            DropIndex("dbo.MissionLogs", new[] { "Mission_Id" });
            DropIndex("dbo.MissionDetails", new[] { "Id" });
            DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
            DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
            AddColumn("dbo.Members", "BirthDate", c => c.DateTime());
            AddColumn("dbo.Members", "Gender", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "WacLevel", c => c.Int(nullable: false));
            AlterColumn("dbo.AnimalOwners", "Animal_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.AnimalOwners", "Owner_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.MissionLogs", "Mission_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.UnitMemberships", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Street", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "City", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "State", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Zip", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.PersonContacts", "Person_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.AnimalOwners", "Animal_Id");
            CreateIndex("dbo.AnimalOwners", "Owner_Id");
            CreateIndex("dbo.MissionLogs", "Mission_Id");
            CreateIndex("dbo.MissionDetails", "Id");
            CreateIndex("dbo.PersonAddresses", "Person_Id");
            CreateIndex("dbo.PersonContacts", "Person_Id");
            CreateIndex("dbo.UnitMemberships", "Person_Id");
            AddForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MissionDetails", "Id", "dbo.Missions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);

            Sql("UPDATE dbo.Members SET BirthDate = InternalBirthDate");
            Sql("UPDATE dbo.Members SET WacLevel = InternalWacLevel");
            Sql("UPDATE dbo.Members SET Gender = CASE InternalGender WHEN 'm' THEN 1 WHEN 'f' THEN 2 ELSE 0 END");
            DropColumn("dbo.Members", "InternalBirthDate");
            DropColumn("dbo.Members", "InternalGender");
            DropColumn("dbo.Members", "InternalWacLevel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Members", "InternalWacLevel", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "InternalGender", c => c.String());
            AddColumn("dbo.Members", "InternalBirthDate", c => c.DateTime());
            DropForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.MissionDetails", "Id", "dbo.Missions");
            DropForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions");
            DropForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals");
            DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
            DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
            DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
            DropIndex("dbo.MissionDetails", new[] { "Id" });
            DropIndex("dbo.MissionLogs", new[] { "Mission_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
            AlterColumn("dbo.PersonContacts", "Person_Id", c => c.Guid());
            AlterColumn("dbo.PersonAddresses", "Person_Id", c => c.Guid());
            AlterColumn("dbo.PersonAddresses", "Zip", c => c.String());
            AlterColumn("dbo.PersonAddresses", "State", c => c.String());
            AlterColumn("dbo.PersonAddresses", "City", c => c.String());
            AlterColumn("dbo.PersonAddresses", "Street", c => c.String());
            AlterColumn("dbo.UnitMemberships", "Person_Id", c => c.Guid());
            AlterColumn("dbo.MissionLogs", "Mission_Id", c => c.Guid());
            AlterColumn("dbo.AnimalOwners", "Owner_Id", c => c.Guid());
            AlterColumn("dbo.AnimalOwners", "Animal_Id", c => c.Guid());

            Sql("UPDATE dbo.Members SET InternalBirthDate = BirthDate");
            Sql("UPDATE dbo.Members SET InternalWacLevel = WacLevel");
            Sql("UPDATE dbo.Members SET InternalGender = CASE Gender WHEN 1 THEN 'm' WHEN 2 THEN 'f' ELSE NULL END");

            DropColumn("dbo.Members", "WacLevel");
            DropColumn("dbo.Members", "Gender");
            DropColumn("dbo.Members", "BirthDate");
            CreateIndex("dbo.UnitMemberships", "Person_Id");
            CreateIndex("dbo.PersonContacts", "Person_Id");
            CreateIndex("dbo.PersonAddresses", "Person_Id");
            CreateIndex("dbo.MissionDetails", "Id");
            CreateIndex("dbo.MissionLogs", "Mission_Id");
            CreateIndex("dbo.AnimalOwners", "Owner_Id");
            CreateIndex("dbo.AnimalOwners", "Animal_Id");
            AddForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.MissionDetails", "Id", "dbo.Missions", "Id");
            AddForeignKey("dbo.MissionLogs", "Mission_Id", "dbo.Missions", "Id");
            AddForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals", "Id");
        }
    }
}
