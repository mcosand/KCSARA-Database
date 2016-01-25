namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class EncryptMethod : DbMigration
  {
    public override void Up()
    {
      DropForeignKey("dbo.MemberEmergencyContacts", "Member_Id", "dbo.Members");
      DropIndex("dbo.MemberEmergencyContacts", new[] { "Member_Id" });
      RenameColumn(table: "dbo.MemberEmergencyContacts", name: "Member_Id", newName: "MemberId");
      AddColumn("dbo.MemberEmergencyContacts", "EncryptionType", c => c.Int(nullable: false));
      AddColumn("dbo.MemberMedicals", "EncryptionType", c => c.Int(nullable: false));

      Sql("Update dbo.MemberEmergencyContacts SET EncryptionType=1");
      Sql("Update dbo.MemberMedicals SET EncryptionType=1");
      Sql("DELETE FROM dbo.MemberEmergencyContacts WHERE MemberId IS NULL");

      AlterColumn("dbo.MemberEmergencyContacts", "MemberId", c => c.Guid(nullable: false));
      CreateIndex("dbo.MemberEmergencyContacts", "MemberId");
      AddForeignKey("dbo.MemberEmergencyContacts", "MemberId", "dbo.Members", "Id", cascadeDelete: true);
    }

    public override void Down()
    {
      DropForeignKey("dbo.MemberEmergencyContacts", "MemberId", "dbo.Members");
      DropIndex("dbo.MemberEmergencyContacts", new[] { "MemberId" });
      AlterColumn("dbo.MemberEmergencyContacts", "MemberId", c => c.Guid());
      DropColumn("dbo.MemberMedicals", "EncryptionType");
      DropColumn("dbo.MemberEmergencyContacts", "EncryptionType");
      RenameColumn(table: "dbo.MemberEmergencyContacts", name: "MemberId", newName: "Member_Id");
      CreateIndex("dbo.MemberEmergencyContacts", "Member_Id");
      AddForeignKey("dbo.MemberEmergencyContacts", "Member_Id", "dbo.Members", "Id");
    }
  }
}
