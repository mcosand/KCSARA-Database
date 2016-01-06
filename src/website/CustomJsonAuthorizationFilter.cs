/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System.Threading.Tasks;
  using Microsoft.AspNet.Mvc;
  using Microsoft.AspNet.Mvc.Filters;

  public class CustomJsonAuthorizationFilter : IAsyncAuthorizationFilter
  {
    private AuthorizeFilter wrappedFilter;
    public CustomJsonAuthorizationFilter(AuthorizeFilter wrappedFilter)
    {
      this.wrappedFilter = wrappedFilter;
    }

    public async Task OnAuthorizationAsync(Microsoft.AspNet.Mvc.Filters.AuthorizationContext context)
    {
      await this.wrappedFilter.OnAuthorizationAsync(context);
      if (context.Result != null && IsAjaxRequest(context))
      {
        context.Result = new JsonResult("please login");
        context.HttpContext.Response.StatusCode = 403;

        //context.Result = new JsonResult(new
        //{
        //  success = false,
        //  error = "You must be signed in."
        //});
      }
      return;
    }

    //This could be an extension method of the HttpContext/HttpRequest
    private bool IsAjaxRequest(Microsoft.AspNet.Mvc.Filters.AuthorizationContext filterContext)
    {
      return filterContext.HttpContext.Request.Path.StartsWithSegments(new Microsoft.AspNet.Http.PathString("/api"));
    }
  }
}
