namespace Kcsar.Database.Model.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorValidate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals");
            DropForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members");
            DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
            DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
            DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
            DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
            AlterColumn("dbo.AnimalOwners", "Animal_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.AnimalOwners", "Owner_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.UnitMemberships", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Street", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "City", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "State", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Zip", c => c.String(nullable: false));
            AlterColumn("dbo.PersonAddresses", "Person_Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.PersonContacts", "Person_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.AnimalOwners", "Animal_Id");
            CreateIndex("dbo.AnimalOwners", "Owner_Id");
            CreateIndex("dbo.PersonAddresses", "Person_Id");
            CreateIndex("dbo.PersonContacts", "Person_Id");
            CreateIndex("dbo.UnitMemberships", "Person_Id");
            AddForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members");
            DropForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members");
            DropForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals");
            DropIndex("dbo.UnitMemberships", new[] { "Person_Id" });
            DropIndex("dbo.PersonContacts", new[] { "Person_Id" });
            DropIndex("dbo.PersonAddresses", new[] { "Person_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Owner_Id" });
            DropIndex("dbo.AnimalOwners", new[] { "Animal_Id" });
            AlterColumn("dbo.PersonContacts", "Person_Id", c => c.Guid());
            AlterColumn("dbo.PersonAddresses", "Person_Id", c => c.Guid());
            AlterColumn("dbo.PersonAddresses", "Zip", c => c.String());
            AlterColumn("dbo.PersonAddresses", "State", c => c.String());
            AlterColumn("dbo.PersonAddresses", "City", c => c.String());
            AlterColumn("dbo.PersonAddresses", "Street", c => c.String());
            AlterColumn("dbo.UnitMemberships", "Person_Id", c => c.Guid());
            AlterColumn("dbo.AnimalOwners", "Owner_Id", c => c.Guid());
            AlterColumn("dbo.AnimalOwners", "Animal_Id", c => c.Guid());
            CreateIndex("dbo.UnitMemberships", "Person_Id");
            CreateIndex("dbo.PersonContacts", "Person_Id");
            CreateIndex("dbo.PersonAddresses", "Person_Id");
            CreateIndex("dbo.AnimalOwners", "Owner_Id");
            CreateIndex("dbo.AnimalOwners", "Animal_Id");
            AddForeignKey("dbo.UnitMemberships", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.PersonContacts", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.PersonAddresses", "Person_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.AnimalOwners", "Owner_Id", "dbo.Members", "Id");
            AddForeignKey("dbo.AnimalOwners", "Animal_Id", "dbo.Animals", "Id");
        }
    }
}
