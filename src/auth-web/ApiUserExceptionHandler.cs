/*
 * Copyright Matthew Cosand
  */
namespace Sar.Auth
{
  using System.Net;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web.Http;
  using System.Web.Http.ExceptionHandling;

  public class ApiUserExceptionHandler : IExceptionHandler
  {
    public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
    {
      var exception = context.Exception as UserErrorException;
      if (exception == null)
      {
        context.Result = new TextPlainErrorResult(context.ExceptionContext.Request, HttpStatusCode.InternalServerError, "A server error has occurred.");
        return Task.FromResult(0);
      }
      else
      {
        context.Result = new TextPlainErrorResult(context.ExceptionContext.Request, HttpStatusCode.BadRequest, exception.ExternalMessage);
        return Task.FromResult(0);
      }
    }

    private class TextPlainErrorResult : IHttpActionResult
    {
      public TextPlainErrorResult(HttpRequestMessage request, HttpStatusCode status, string content)
      {
        Request = request;
        Status = status;
        Content = content;
      }

      public HttpRequestMessage Request { get; set; }

      public string Content { get; set; }

      public HttpStatusCode Status { get; set; }

      public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
      {
        HttpResponseMessage response = new HttpResponseMessage(Status);
        response.Content = new StringContent(Content);
        response.RequestMessage = Request;
        return Task.FromResult(response);
      }
    }
  }
}