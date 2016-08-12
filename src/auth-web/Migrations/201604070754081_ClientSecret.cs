namespace Sar.Auth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClientSecret : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientRoles",
                c => new
                    {
                        RoleRow_Id = c.String(nullable: false, maxLength: 25),
                        ClientRow_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.RoleRow_Id, t.ClientRow_Id })
                .ForeignKey("dbo.Roles", t => t.RoleRow_Id, cascadeDelete: true)
                .ForeignKey("dbo.Clients", t => t.ClientRow_Id, cascadeDelete: true)
                .Index(t => t.RoleRow_Id)
                .Index(t => t.ClientRow_Id);
            
            AddColumn("dbo.Clients", "Secret", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientRoles", "ClientRow_Id", "dbo.Clients");
            DropForeignKey("dbo.ClientRoles", "RoleRow_Id", "dbo.Roles");
            DropIndex("dbo.ClientRoles", new[] { "ClientRow_Id" });
            DropIndex("dbo.ClientRoles", new[] { "RoleRow_Id" });
            DropColumn("dbo.Clients", "Secret");
            DropTable("dbo.ClientRoles");
        }
    }
}
