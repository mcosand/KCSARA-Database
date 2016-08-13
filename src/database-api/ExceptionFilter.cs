namespace Kcsara.Database.Api
{
  using System.Web.Http.Filters;
  using log4net;

  public class ExceptionFilter : ExceptionFilterAttribute
  {
    public override void OnException(HttpActionExecutedContext context)
    {
      LogManager.GetLogger("Kcsara.Database.Api").Error(context.Request.RequestUri.AbsoluteUri, context.Exception);
      base.OnException(context);
    }
  }
}
