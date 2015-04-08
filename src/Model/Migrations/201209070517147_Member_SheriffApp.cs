/*
 * Copyright 2012-2014 Matthew Cosand
 */
namespace Kcsar.Database.ta.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Member_SheriffApp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "SheriffApp", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Members", "SheriffApp");
        }
    }
}
