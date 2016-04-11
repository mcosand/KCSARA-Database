/*
 * Copyright Matthew Cosand
 * Based on https://github.com/IdentityServer/IdentityServer3/issues/1124
 */
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Sar.Web
{
  public class NonceCleanupOpenIdConnectAuthenticationHandler : OpenIdConnectAuthenticationHandler
  {
    private readonly ILogger _logger;

    public NonceCleanupOpenIdConnectAuthenticationHandler(ILogger logger) : base(logger)
    {
      _logger = logger;
    }

    protected override void RememberNonce(OpenIdConnectMessage message, string nonce)
    {
      var oldNonces = Request.Cookies.Where(kvp => kvp.Key.StartsWith(OpenIdConnectAuthenticationDefaults.CookiePrefix + "nonce")).ToArray();
      if (oldNonces.Length > 0)
      {
        _logger.WriteInformation("Removing " + oldNonces.Length + " old nonce cookies. Request IP: " + Request.RemoteIpAddress);
        CookieOptions cookieOptions = new CookieOptions
        {
          HttpOnly = true,
          Secure = Request.IsSecure
        };
        foreach (KeyValuePair<string, string> oldNonce in oldNonces)
        {
          Response.Cookies.Delete(oldNonce.Key, cookieOptions);
        }
      }
      base.RememberNonce(message, nonce);
    }
  }
}
