/*
 * Copyright 2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.Controllers
{
  using Kcsar.Database.Model;
  using Kcsara.Database.Services;
  using log4net;

  /// <summary>Container for arguments used by the base controllers.</summary>
  public class ControllerArgs
  {
    /// <summary>Default constructor.</summary>
    /// <param name="db"></param>
    /// <param name="permissions"></param>
    /// <param name="log"></param>
    /// <param name="appSettings"></param>
    public ControllerArgs(
      IKcsarContext db,
      IAuthService permissions,
      ILog log,
      IAppSettings appSettings
      )
    {
      this.log = log;
      this.db = db;
      this.appSettings = appSettings;
      this.permissions = permissions;
    }
    public readonly ILog log;
    public readonly IAppSettings appSettings;
    public readonly IKcsarContext db;
    public readonly IAuthService permissions;
  }
}