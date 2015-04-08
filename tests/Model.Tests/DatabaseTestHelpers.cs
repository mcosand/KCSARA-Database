/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Data
{
  using System;
  using System.Data.Entity.Migrations;
  using System.Diagnostics;
  using System.IO;
  using Kcsar.Database.Data;
  using Microsoft.Win32;

  public static class DatabaseTestHelpers
  {
    private static bool? localDbAvailable = null;
    
    public static bool IsLocalDBAvailable()
    {
      if (localDbAvailable == null)
      {
        try
        {
          RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\12.0");

          localDbAvailable = key != null;
        }
        catch (Exception e)
        {
          Console.WriteLine(e.ToString());
          localDbAvailable = false;
        }
      }
      return (bool)localDbAvailable;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The connection string for the newly created database</returns>
    public static string CreateTestDatabase()
    {      
      if (DatabaseTestHelpers.IsLocalDBAvailable() == false)
      {
        throw new InvalidOperationException("Can't create a new test database when localDb is not available");
      }

      //string dbPrefix = Guid.NewGuid().ToString().ToLowerInvariant().Substring(0, 30);

      AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
      string connString = @"Data Source=(LocalDB)\ProjectsV12;AttachDbFilename=|DataDirectory|\foo.mdf;Integrated Security=True;Connect Timeout=30";
      Console.WriteLine("Using local database: " + connString);
      Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
      Stopwatch timer = new Stopwatch();
      timer.Start();

      var migrate = new System.Data.Entity.Migrations.DbMigrator(new TestConfiguration(connString));
      migrate.Update();
      //var dacService = new DacServices(connString);
      //var dacPackage = DacPackage.Load(System.IO.Directory.GetCurrentDirectory() + "\\Client.Database.dacpac");
      //dacService.Deploy(dacPackage, dbPrefix, true, new DacDeployOptions { CreateNewDatabase = true });

      timer.Stop();
      Console.WriteLine("Database setup took {0}s", timer.Elapsed.TotalSeconds);

      return connString;
    }

    public static void DeleteDatabase(string connString)
    {
      using (var context = new KcsarContext(connString, Console.WriteLine))
      {
        context.Database.Delete();
      }
    }

    class TestConfiguration : DbMigrationsConfiguration<KcsarContext>
    {
      public TestConfiguration(string connString)
      {
        AutomaticMigrationDataLossAllowed = true;
        AutomaticMigrationsEnabled = true;
        TargetDatabase = new System.Data.Entity.Infrastructure.DbConnectionInfo(connString, "System.Data.SqlClient");
      }
    }
  }
}
