namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameAuthSchema : DbMigration
    {
        public override void Up()
        {
            MoveTable(name: "auth.Accounts", newSchema: "authold");
            MoveTable(name: "auth.ExternalLogins", newSchema: "authold");
            MoveTable(name: "auth.LoginLog", newSchema: "authold");
            MoveTable(name: "auth.AccountResetTokens", newSchema: "authold");
            MoveTable(name: "auth.Roles", newSchema: "authold");
            MoveTable(name: "auth.Clients", newSchema: "authold");
            MoveTable(name: "auth.ClientLogoutUris", newSchema: "authold");
            MoveTable(name: "auth.ClientUris", newSchema: "authold");
            MoveTable(name: "auth.Consents", newSchema: "authold");
            MoveTable(name: "auth.Tokens", newSchema: "authold");
            MoveTable(name: "auth.Verifications", newSchema: "authold");
            MoveTable(name: "auth.AccountRoles", newSchema: "authold");
            MoveTable(name: "auth.RoleRoles", newSchema: "authold");
            MoveTable(name: "auth.ClientRoles", newSchema: "authold");
            MoveTable(name: "auth.RoleOwners", newSchema: "authold");
        }
        
        public override void Down()
        {
            MoveTable(name: "authold.RoleOwners", newSchema: "auth");
            MoveTable(name: "authold.ClientRoles", newSchema: "auth");
            MoveTable(name: "authold.RoleRoles", newSchema: "auth");
            MoveTable(name: "authold.AccountRoles", newSchema: "auth");
            MoveTable(name: "authold.Verifications", newSchema: "auth");
            MoveTable(name: "authold.Tokens", newSchema: "auth");
            MoveTable(name: "authold.Consents", newSchema: "auth");
            MoveTable(name: "authold.ClientUris", newSchema: "auth");
            MoveTable(name: "authold.ClientLogoutUris", newSchema: "auth");
            MoveTable(name: "authold.Clients", newSchema: "auth");
            MoveTable(name: "authold.Roles", newSchema: "auth");
            MoveTable(name: "authold.AccountResetTokens", newSchema: "auth");
            MoveTable(name: "authold.LoginLog", newSchema: "auth");
            MoveTable(name: "authold.ExternalLogins", newSchema: "auth");
            MoveTable(name: "authold.Accounts", newSchema: "auth");
        }
    }
}
