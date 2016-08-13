/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsar.Database.Model.Migrations
{
  using System;
  using System.Collections.Generic;
  using System.Data.Entity.Infrastructure;
  using System.Data.Entity.Migrations;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  
  public static class Migrator
  {
    public static void UpdateDatabase(string connectionString)
    {
      var configuration = new Configuration
      {
        TargetDatabase = new DbConnectionInfo(connectionString, "System.Data.SqlClient")        
      };

      var migrator = new DbMigrator(configuration);     
      var pending = migrator.GetPendingMigrations().ToArray();
      var local = migrator.GetLocalMigrations().ToArray();
      var db = migrator.GetDatabaseMigrations().ToArray();
      migrator.Update();
    }
  }
}
