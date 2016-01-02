/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System;
  using Microsoft.AspNet.Mvc;
  using Microsoft.AspNet.Mvc.Filters;

  public class HandleExceptionAttribute : Attribute, IExceptionFilter
  {
    public void OnException(ExceptionContext context)
    {
      var ex = context.Exception as StatusCodeException;
      if (ex != null)
      {
        var aggEx = ex as ModelErrorsException;
        context.Result = new JsonResult(aggEx == null ? (object)ex.Message : new { Message = ex.Message, Errors = aggEx.Errors }, Utils.GetJsonSettings());
        context.HttpContext.Response.StatusCode = (int)ex.StatusCode;
        context.Exception = null;
      }
    }
  }
}
