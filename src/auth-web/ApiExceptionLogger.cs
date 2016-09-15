namespace Sar.Auth
{
  using System.Security.Claims;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web.Http.ExceptionHandling;
  using Serilog;

  public class ApiExceptionLogger : IExceptionLogger
  {
    private readonly ILogger _log;

    public ApiExceptionLogger(ILogger log)
    {
      _log = log;
    }

    public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
    {
      var identity = context.RequestContext?.Principal?.Identity as ClaimsIdentity;
      
      if (context.Exception is UserErrorException)
      {
        _log.Debug(context.Exception, "User error by {user}", identity?.Name);
      }
      _log.Error(context.Exception, "Unhandled API exception {user}", identity?.Name);
      return Task.FromResult(0);
    }
  }
}