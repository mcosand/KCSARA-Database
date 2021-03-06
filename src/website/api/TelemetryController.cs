﻿/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System.Web.Http;
using Kcsar.Database.Model;
using Kcsara.Database.Web.Model;
using log4net;

namespace Kcsara.Database.Web.api
{
  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  public class TelemetryController : BaseApiController
  {
    public TelemetryController(IKcsarContext db, Services.IAuthService auth, ILog log)
      : base(db, auth, log)
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
