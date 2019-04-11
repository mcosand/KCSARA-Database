using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Kcsara.Database.Api;
using log4net;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Ninject;
using Owin;
using Sar;
using Sar.Database.Services;
using Sar.Web;

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

      app.Map("/api2", apiApp => apiApp.UseDatabaseApi(kernel));

      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = "Cookies"
      });

      NameValueCollection configStrings = ConfigurationManager.AppSettings;

      app.Use<NonceCleanupOpenIdConnectAuthenticationMiddleware>(app, new OpenIdConnectAuthenticationOptions
      {
        Authority = configStrings["auth:authority"].Trim('/') + "/",
        ClientId = configStrings["auth:clientId"],
        RedirectUri = configStrings["auth:redirect"].Trim('/') + "/",
        ResponseType = "code id_token token",
        Scope = "openid email profile",
        PostLogoutRedirectUri = configStrings["auth:redirect"].Trim('/') + "/",
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

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
              // For some reason the auth grant was invalid. Send the user to the destination page
              // so they can re-generate the grant.
              LogManager.GetLogger("Startup").ErrorFormat("No access token returned for grant {0}", n.Code);
              n.Response.Redirect(n.RedirectUri);
              n.HandleResponse();
              return;
            }

            // use the access token to retrieve claims from userinfo
            var userInfoClient = new UserInfoClient(
            new Uri(configStrings["auth:authority"].Trim('/') + "/connect/userinfo"),
            tokenResponse.AccessToken);

            var userInfoResponse = await userInfoClient.GetAsync();

            // create new identity
            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
            id.AddClaims(userInfoResponse.GetClaimsIdentity().Claims);

            var accountId = new Guid(id.FindFirst("sub").Value);

            var rolesService = kernel.Get<IRolesService>();
            foreach (var role in rolesService.ListAllRolesForAccount(accountId))
            {
              id.AddClaim(new Claim("role", role));
            }

            id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
            id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString()));
            if (!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
              id.AddClaim(new Claim("refresh_token", tokenResponse.RefreshToken));
            }
            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

            id.AddClaim(new Claim("sid", n.JwtSecurityToken.Claims.First(f => f.Type == "sid").Value));

            n.AuthenticationTicket = new AuthenticationTicket(
                new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, "name", "role"),
                n.AuthenticationTicket.Properties);

          },

          AuthenticationFailed = n =>
          {
            LogManager.GetLogger(typeof(Startup)).Debug("Auth Failed", n.Exception);
            return Task.FromResult(0);
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
    }
  }
}
