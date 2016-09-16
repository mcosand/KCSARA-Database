using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Kcsara.Database.Api;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Ninject;
using Owin;
using Sar.Auth;
using Sar.Services;

[assembly: OwinStartup(typeof(Kcsara.Database.Web.Startup))]

namespace Kcsara.Database.Web
{
  public class Startup
  {
    // Lives here so other bootstrap system can replace it.
    internal static IKernel kernel = new StandardKernel();

    public void Configuration(IAppBuilder app)
    {
      var config = kernel.Get<IHost>();

      var signingCertificate = Cert.Load(config.GetConfig("cert:key"), kernel.Get<ILog>().Warn);
      app.Map("/auth", authApp => authApp.UseAuthServer(kernel, signingCertificate));
      app.Map("/api2", apiApp => apiApp.UseDatabaseApi(kernel, signingCertificate));

      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = "Cookies"
      });

      app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

      NameValueCollection configStrings = ConfigurationManager.AppSettings;
      var addScopes = new[] { "database-api" };


      app.Use<NonceCleanupOpenIdConnectAuthenticationMiddleware>(app, new OpenIdConnectAuthenticationOptions
      {
        Authority = configStrings["auth:authority"].Trim('/') + "/",
        ClientId = configStrings["auth:clientId"],
        RedirectUri = configStrings["auth:redirect"].Trim('/') + "/",
        ResponseType = "code id_token token",
        Scope = "openid email profile kcsara-profile" + (addScopes.Length > 0 ? " " + string.Join(" ", addScopes) : string.Empty),
        TokenValidationParameters = new TokenValidationParameters
        {
          NameClaimType = "name",
          RoleClaimType = "role"
        },
        SignInAsAuthenticationType = "Cookies",
        Notifications = new OpenIdConnectAuthenticationNotifications
        {
          AuthorizationCodeReceived = async n =>
          {
            // use the code to get the access and refresh token
            var tokenClient = new TokenClient(
            configStrings["auth:authority"].Trim('/') + "/connect/token",
            configStrings["auth:clientId"],
            configStrings["auth:secret"]);

            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(
                n.Code, n.RedirectUri);

            // use the access token to retrieve claims from userinfo
            var userInfoClient = new UserInfoClient(
            new Uri(configStrings["auth:authority"].Trim('/') + "/connect/userinfo"),
            tokenResponse.AccessToken);

            var userInfoResponse = await userInfoClient.GetAsync();

            // create new identity
            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
            id.AddClaims(userInfoResponse.GetClaimsIdentity().Claims);

            id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
            id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
            if (!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
              id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
            }
            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
            id.AddClaim(new Claim("sid", n.AuthenticationTicket.Identity.FindFirst("sid").Value));

            n.AuthenticationTicket = new AuthenticationTicket(
                new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, "name", "role"),
                n.AuthenticationTicket.Properties);
          },

          RedirectToIdentityProvider = n =>
          {
            // if signing out, add the id_token_hint
            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            {
              var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

              if (idTokenHint != null)
              {
                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
              }

            }

            return Task.FromResult(0);
          }
        }
      });

      //app.Map("/api", apiApp =>
      //{
      //  var config = new HttpConfiguration();
      //  WebApiConfig.Register(config, kernel);
      //  app.UseWebApi(config);
      //});
    }
  }
}
