/*
 * Copyright 2012-2014 Matthew Cosand
 */
using Kcsar.Database.Model;
using Kcsara.Database.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;

namespace Kcsara.Database.Web.api
{
  /// <summary>Provides telemetry back to server. Not for general use.</summary>
  public class TelemetryController : BaseApiController
  {
    public TelemetryController(IKcsarContext db, ILog log)
      : base(db, log)
    { }

    /// <summary>Client trapped a generic unhandled error.</summary>
    /// <param name="error">Error details.</param>
    /// <returns>true</returns>
    [AllowAnonymous]
    public bool Error([FromBody]TelemetryErrorView error)
    {
      log.DebugFormat("{3} CLIENT ERROR: {0} // {1} // {2} - {4}", error.Error, error.Message, error.Location, User.Identity.Name, Request.Headers.UserAgent);
      return true;
    }
  }
}
