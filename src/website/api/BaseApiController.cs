/*
 * Copyright 2012-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Web.Http;
  using System.Web.Http.Results;
  using Kcsar.Database.Model;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.Model;
  using log4net;
  using Newtonsoft.Json;

  public abstract class BaseApiController : ApiController, IDisposable
  {
    public BaseApiController(IKcsarContext db, ILog log)
      : this(db, Ninject.ResolutionExtensions.Get<IAuthService>(MvcApplication.myKernel), log)
    {
    }

    public BaseApiController(IKcsarContext db, IAuthService auth, ILog log)
      : base()
    {
      this.db = db;
      this.log = log;
      this.Permissions = auth;
    }

    public IAuthService Permissions = null;
    public const string ModelRootNodeName = "_root";

    protected readonly IKcsarContext db;
    protected readonly ILog log;

    protected override ResponseMessageResult ResponseMessage(HttpResponseMessage response)
    {
      var r = base.ResponseMessage(response);
      var authInfo = new
      {
        username = this.User.Identity.Name,
        isAuthenticated = this.User.Identity.IsAuthenticated
      };
      var authInfoCookie = new CookieHeaderValue("authInfo", JsonConvert.SerializeObject(authInfo));
      authInfoCookie.Expires = DateTime.Now.AddHours(1);

      r.Response.Headers.AddCookies(new[] { authInfoCookie });
      return r;
    }

    protected override void Dispose(bool disposing)
    {
      IDisposable disposeDb = this.db as IDisposable;
      if (disposing && disposeDb != null) disposeDb.Dispose();
      base.Dispose(disposing);
    }

    protected void ThrowAuthError()
    {
      log.WarnFormat("AUTH ERR: {0} {1}", User.Identity.Name, Request.RequestUri);
      throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
    }

    protected T GetObjectOrNotFound<T>(Func<T> getter)
    {
      var result = getter();
      if (result == null)
      {
        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
      }
      return result;
    }

    protected void ThrowSubmitErrors(IEnumerable<SubmitError> errors)
    {
      log.WarnFormat("{0} {1} {2}", Request.RequestUri, User.Identity.Name, JsonConvert.SerializeObject(errors));
      var errObject = errors.ToDictionary(f => f.Property, f => f.Error);
      throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, errObject));
    }

    protected string GetDateFormat()
    {
      return "{0:yyyy-MM-dd}";
    }
  }
}
