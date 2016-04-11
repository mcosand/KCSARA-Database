/*
 * Copyright Matthew Cosand
 */
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace Sar.Web
{
  public class NonceCleanupOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
  {
    private readonly ILogger _logger;

    public NonceCleanupOpenIdConnectAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, OpenIdConnectAuthenticationOptions options) : base(next, app, options)
    {
      _logger = app.CreateLogger<NonceCleanupOpenIdConnectAuthenticationMiddleware>();
    }

    protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
    {
      return new NonceCleanupOpenIdConnectAuthenticationHandler(_logger);
    }
  }
}
