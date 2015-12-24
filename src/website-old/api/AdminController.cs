/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Configuration;
  using System.Data.SqlClient;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Web.Http;
  using Kcsar.Database.Model;
  using log4net;

  [ModelValidationFilter]
  public class AdminController : BaseApiController
  {
    public AdminController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    [HttpPost]
    [Authorize(Roles="cdb.admins")]
    public string Sql(Models.Admin.ExecuteDatabaseScript model)
    {
      StringBuilder result = new StringBuilder("Starting database script ...\n");

      var key = ConfigurationManager.AppSettings["DatabaseUpdateKey"];
      if (string.IsNullOrWhiteSpace(key))
      {
        return "DatabaseUpdateKey not in AppSettings";
      }

      if (!string.Equals(key, model.UpdateKey))
      {
        return "UpdateKey is not correct";
      }

      if (model.Store == "AuthStore" && !Permissions.IsInRole("site.accounts"))
      {
        return "No permission";
      }

      try
      {
        string connectionString = ConfigurationManager.ConnectionStrings[model.Store].ConnectionString;
        using (var conn = CreateAndOpenConnection(connectionString, result))
        {
          ExecuteSqlFile(conn, model.Sql, result);
        }
      }
      catch (Exception ex)
      {
        result.AppendLine(ex.ToString());
      }
      return result.ToString();
    }

    public static SqlConnection CreateAndOpenConnection(string connectionString, StringBuilder responseText)
    {
      SqlConnection conn = new SqlConnection(connectionString);
      conn.InfoMessage += (object sender, SqlInfoMessageEventArgs e) => responseText.AppendLine(e.Message);      
      conn.Open();
      return conn;
    }

    public static void ExecuteSqlFile(SqlConnection conn, string cmdText, StringBuilder result)
    {
      string[] commands;

      commands = Regex.Split(cmdText, "^GO\\r?\\n", RegexOptions.Multiline);
      foreach (var command in commands)
      {
        if (string.IsNullOrWhiteSpace(command)) continue;

        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
          result.AppendFormat("({0}) rows affected.\n", cmd.ExecuteNonQuery());
        }
      }
    }

  }
}
