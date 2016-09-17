namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schema : DbMigration
    {
        public override void Up()
        {
            MoveTable(name: "dbo.Accounts", newSchema: "auth");
            MoveTable(name: "dbo.LoginLog", newSchema: "auth");
            MoveTable(name: "dbo.Roles", newSchema: "auth");
            MoveTable(name: "dbo.Clients", newSchema: "auth");
            MoveTable(name: "dbo.ClientLogoutUris", newSchema: "auth");
            MoveTable(name: "dbo.ClientUris", newSchema: "auth");
            MoveTable(name: "dbo.ExternalLogins", newSchema: "auth");
            MoveTable(name: "dbo.Verifications", newSchema: "auth");
            MoveTable(name: "dbo.AccountRoles", newSchema: "auth");
            MoveTable(name: "dbo.RoleRoles", newSchema: "auth");
            MoveTable(name: "dbo.ClientRoles", newSchema: "auth");
            MoveTable(name: "dbo.RoleOwners", newSchema: "auth");
        }
        
        public override void Down()
        {
            MoveTable(name: "auth.RoleOwners", newSchema: "dbo");
            MoveTable(name: "auth.ClientRoles", newSchema: "dbo");
            MoveTable(name: "auth.RoleRoles", newSchema: "dbo");
            MoveTable(name: "auth.AccountRoles", newSchema: "dbo");
            MoveTable(name: "auth.Verifications", newSchema: "dbo");
            MoveTable(name: "auth.ExternalLogins", newSchema: "dbo");
            MoveTable(name: "auth.ClientUris", newSchema: "dbo");
            MoveTable(name: "auth.ClientLogoutUris", newSchema: "dbo");
            MoveTable(name: "auth.Clients", newSchema: "dbo");
            MoveTable(name: "auth.Roles", newSchema: "dbo");
            MoveTable(name: "auth.LoginLog", newSchema: "dbo");
            MoveTable(name: "auth.Accounts", newSchema: "dbo");
        }
    }
}
