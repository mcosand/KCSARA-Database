/*
 * Copyright 2012-2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using System.Web.Http;
  using Kcsar.Database.Data;
  using Kcsara.Database.Web.Model;
  using log4net;

  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  public class TelemetryController : BaseApiController
  {
    public TelemetryController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    /// <summary>Client trapped a generic unhandled error.</summary>
    /// <param name="error">Error details.</param>
    /// <returns>true</returns>
    public bool Error([FromBody]TelemetryErrorView error)
    {
      log.DebugFormat("{3} CLIENT ERROR: {0} // {1} // {2} - {4}", error.Error, error.Message, error.Location, User.Identity.Name, Request.Headers.UserAgent);
      return true;
    }
  }
}
