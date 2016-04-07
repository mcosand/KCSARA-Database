using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

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
      
      app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
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
