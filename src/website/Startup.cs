using System.Configuration;
using System.IdentityModel.Tokens;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Sar.Web;

[assembly: OwinStartup(typeof(Kcsara.Database.Web.Startup))]

namespace Kcsara.Database.Web
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = "Cookies"
      });

      app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
      
      app.Use<NonceCleanupOpenIdConnectAuthenticationMiddleware>(app, new OpenIdConnectAuthenticationOptions
      {
        Authority = ConfigurationManager.AppSettings["auth:authority"].Trim('/') + "/",
        ClientId = ConfigurationManager.AppSettings["auth:clientId"],
        RedirectUri = ConfigurationManager.AppSettings["auth:redirect"].Trim('/') + "/",
        ResponseType = "id_token",
        Scope = "openid email profile kcsara-profile",
        TokenValidationParameters = new TokenValidationParameters
        {
          NameClaimType = "name"
        },
        SignInAsAuthenticationType = "Cookies"
      });
    }
  }
}
