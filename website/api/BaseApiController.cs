using Kcsar.Database.Model;
using Kcsara.Database.Web.Model;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Kcsara.Database.Web.api
{
    public abstract class BaseApiController : ApiController, IDisposable
    {
        static BaseApiController()
        {
            GlobalConfiguration.Configuration.Filters.Add(new ExceptionFilter());
        }

        public Kcsara.Database.Web.Controllers.IPermissions Permissions = null;


        public const string ModelRootNodeName = "_root";

        protected KcsarContext db = new KcsarContext();
        protected ILog log = LogManager.GetLogger("Default");

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.PeerInitialize();
        }

        internal virtual void PeerInitialize()
        {
            this.log = LogManager.GetLogger(this.GetType());
            this.Permissions = new Kcsara.Database.Web.Controllers.PermissionsProvider(User, db);
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