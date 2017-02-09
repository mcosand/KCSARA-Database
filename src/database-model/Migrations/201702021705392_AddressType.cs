namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Linq;
  using System.Data.Entity.Migrations;

  public partial class AddressType : DbMigration
  {
    public override void Up()
    {
      AddColumn("dbo.PersonAddresses", "Type", c => c.Int(nullable: false));

      string cases = string.Join(" ", Enum.GetValues(typeof(PersonAddressType)).Cast<int>().Select(f => $"WHEN '{Enum.GetName(typeof(PersonAddressType), f)}' THEN {f}"));
      Sql($"UPDATE dbo.PersonAddresses SET Type=CASE InternalType {cases} ELSE 0 END");
      DropColumn("dbo.PersonAddresses", "InternalType");
    }

    public override void Down()
    {
      AddColumn("dbo.PersonAddresses", "InternalType", c => c.String());
      string cases = string.Join(" ", Enum.GetValues(typeof(PersonAddressType)).Cast<int>().Select(f => $"WHEN {f} THEN '{Enum.GetName(typeof(PersonAddressType), f).ToLower()}'"));
      Sql($"UPDATE dbo.PersonAddresses SET InternalType=CASE Type {cases} ELSE 'other' END");
      DropColumn("dbo.PersonAddresses", "Type");
    }
  }
}
