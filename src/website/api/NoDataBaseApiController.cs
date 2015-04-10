/*
 * Copyright 2015 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Web.Http;
  using Kcsar.Database.Data;
  using Kcsara.Database.Services;
  using Kcsara.Database.Web.Model;
  using log4net;
  using Newtonsoft.Json;

  public abstract class NoDataBaseApiController : ApiController
  {
    public NoDataBaseApiController(ILog log)
      : this(Ninject.ResolutionExtensions.Get<IAuthService>(MvcApplication.myKernel), log)
    {
    }

    public NoDataBaseApiController(IAuthService auth, ILog log)
      : base()
    {
      this.log = log;
      this.Permissions = auth;
    }

    public IAuthService Permissions = null;


    public const string ModelRootNodeName = "_root";

    protected readonly ILog log;

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
