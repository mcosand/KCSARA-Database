namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Data.Entity.Migrations;

  public partial class SarUnitApplyStatus : DbMigration
  {
    public override void Up()
    {
      RenameColumn("dbo.SarUnits", "NoApplicationsText", "ApplicationsText");
      AddColumn("dbo.SarUnits", "ApplicationStatus", c => c.Int(nullable: false));
      Sql("UPDATE dbo.SarUnits SET ApplicationStatus=" + (int)ApplicationStatus.Yes);
      Sql("UPDATE dbo.SarUnits SET ApplicationsText=NULL, ApplicationStatus=" + (int)ApplicationStatus.No + " WHERE ApplicationsText='never'");
    }

    public override void Down()
    {
      RenameColumn("dbo.SarUnits", "ApplicationsText", "NoApplicationsText");
      DropColumn("dbo.SarUnits", "ApplicationStatus");
    }
  }
}
