using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Kcsara.Database.Api;
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

      NameValueCollection configStrings = ConfigurationManager.AppSettings;

      app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
        ExpireTimeSpan = TimeSpan.FromHours(1)
      });

      JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

      string identityUrl = configStrings["auth:authority"].Trim('/') + "/";

      app.Use<NonceCleanupOpenIdConnectAuthenticationMiddleware>(app, new OpenIdConnectAuthenticationOptions
      {
        AuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType,
        SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
        RedirectUri = configStrings["auth:redirect"].Trim('/') + "/signin-oidc",
        PostLogoutRedirectUri = configStrings["auth:redirect"].Trim('/') + "/",
        Authority = identityUrl,

        ClientId = configStrings["auth:clientId"],
        ClientSecret = configStrings["auth:secret"],

        ResponseType = "code id_token",
        Scope = "openid email profile",

        Notifications = new OpenIdConnectAuthenticationNotifications
        {
          AuthorizationCodeReceived = async n =>
          {
            var tokenClient = new TokenClient(
              identityUrl + "connect/token",
              configStrings["auth:clientId"],
              configStrings["auth:secret"]
            );

            var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);

            if (tokenResponse.IsError)
            {
              throw new AuthenticationException(tokenResponse.Error);
            }

            var userInfoClient = new UserInfoClient(new Uri(identityUrl + "connect/userinfo"), tokenResponse.AccessToken);

            var userInfoResponse = await userInfoClient.GetAsync();

            var id = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);
            id.AddClaims(userInfoResponse.Claims.Select(f => new Claim(f.Item1, f.Item2)));

            id.AddClaim(new Claim("access_token", tokenResponse.AccessToken));
            id.AddClaim(new Claim("expires_at", DateTime.Now.AddSeconds(tokenResponse.ExpiresIn).ToLocalTime().ToString(CultureInfo.InvariantCulture)));
            id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
            id.AddClaim(new Claim("sid", n.AuthenticationTicket.Identity.FindFirst("sid").Value));
            var rolesService = kernel.Get<IRolesService>();
            var accountId = new Guid(id.FindFirst("sub").Value);
            foreach (var role in rolesService.ListAllRolesForAccount(accountId))
            {
              id.AddClaim(new Claim("role", role));
            }

            n.AuthenticationTicket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1);
            n.AuthenticationTicket = new AuthenticationTicket(
              new ClaimsIdentity(id.Claims, n.AuthenticationTicket.Identity.AuthenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role),
              n.AuthenticationTicket.Properties
            );
          },

          RedirectToIdentityProvider = n =>
          {
            if (n.ProtocolMessage.RequestType != OpenIdConnectRequestType.LogoutRequest)
            {
              return Task.FromResult(0);
            }

            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

            if (idTokenHint != null)
            {
              n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
            }

            return Task.FromResult(0);
          }
        }
      });
    }
  }
}
