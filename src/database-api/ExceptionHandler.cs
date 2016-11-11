using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sar;

namespace Kcsara.Database.Api
{
  /// <summary>
  /// An exception handler that can process ApiExceptions.
  /// </summary>
  public class ExceptionHandler : IExceptionHandler
  {
    private readonly ILog _log;

    /// <summary>Default constructor</summary>
    /// <param name="log"></param>
    public ExceptionHandler(ILog log)
    {
      if (log == null) throw new ArgumentNullException(nameof(log));

      _log = log;
    }

    /// <summary>
    /// Process an unhandled exception, either allowing it to propagate or handling it by providing a response message
    ///  to return instead.
    /// </summary>
    /// <param name="context">The exception handler context.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous exception handling operation.</returns>
    public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
    {
      var modelErrorException = context.Exception as ModelErrorException;
      if (modelErrorException != null)
      {
        var jsonSettings = context.RequestContext.Configuration.Formatters.JsonFormatter.SerializerSettings;

        var resolver = (DefaultContractResolver)jsonSettings.ContractResolver;

        var jsonBody = JsonConvert.SerializeObject(
          new
          {
            Errors = modelErrorException.PropertyErrors
                        .ToDictionary(
                            f => resolver.GetResolvedPropertyName(f.Key),
                            f => f.Value.Distinct()
                        )
          },
          jsonSettings);

        context.Result = new ResponseMessageResult(context.Request.CreateErrorResponse(HttpStatusCode.BadRequest, jsonBody));
        return Task.FromResult(0);
      }


      // Determining the status code
      var userException = context.Exception as UserErrorException;

      // ServicesClient already logs the appropriate level of details here.
      //if (proxyException == null)
      //{
      if (userException == null || userException.StatusCode >= HttpStatusCode.InternalServerError)
      {
        _log.Error($"Error at {context.Request.Method} {context.Request.RequestUri}", context.Exception);
      }
      else
      {
        _log.Info($"{userException.StatusCode} returned from {context.Request.Method} {context.Request.RequestUri}: {userException.Message}");
      }
      //}

      HttpResponseMessage responseMessage = context.Request.CreateResponse(
          userException?.StatusCode ?? HttpStatusCode.InternalServerError,
          new HttpError(userException?.ExternalMessage ?? "Internal Server Error"));

      // ServiceCallExceptions have already formatted their errors as JSON.
      //if (proxyException != null)
      //{
      //  responseMessage.Content = new StringContent(proxyException.UserMessage, Encoding.UTF8, "application/json");
      //}

      context.Result = new ResponseMessageResult(responseMessage);

      return Task.FromResult(0);
    }
  }
}
