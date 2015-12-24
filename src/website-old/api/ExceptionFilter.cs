/*
 * Copyright 2012-2015 Matthew Cosand
 */
 namespace Kcsara.Database.Web.api
{
  using System.Web.Http.Filters;
  using log4net;

  public class ExceptionFilter : ExceptionFilterAttribute
  {
    public override void OnException(HttpActionExecutedContext context)
    {
      LogManager.GetLogger("Kcsara.Database.Web.api").Error(context.Request.RequestUri.AbsoluteUri, context.Exception);
      base.OnException(context);
    }
  }
}
