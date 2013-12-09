using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using log4net;

namespace Kcsara.Database.Web.api
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            LogManager.GetLogger("Kcsara.Database.Web.api").ErrorFormat("UNHANDLED: {0}", context.Exception.ToString());

            if (context.Response == null)
            {
                context.Response = new HttpResponseMessage();
            }
            context.Response.StatusCode = HttpStatusCode.InternalServerError;
            context.Response.Content = new StringContent("General Error");
            base.OnException(context);
            new WebRequestErrorEventMvc("An unhandled exception has occurred in API.", this, 103005, context.Exception).Raise();
        }
    }
}