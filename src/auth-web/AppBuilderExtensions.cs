using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using Sar;
using Sar.Auth.Data;
using Sar.Database.Web.Auth;
using Sar.Database.Web.Auth.Controllers;
using Sar.Database.Web.Auth.Services;
using Sar.Web;
using Serilog;

namespace Owin
{
  [ExcludeFromCodeCoverage]
  public static class AppBuilderExtensions
  {
    public static void UseAuthServer(this IAppBuilder app, IKernel kernel, X509Certificate2 signingCert)
    {
      //var kernel = WebSetup.SetupDependencyInjection(RegisterServices, ConfigurationManager.AppSettings);
      var config = kernel.Get<IHost>();
      var log = kernel.Get<ILogger>();

      var userService = kernel.Get<SarUserService>();
      var clientStore = kernel.Get<IClientStore>();
      var corsService = new DefaultCorsPolicyService { AllowAll = true };

      var factory = new IdentityServerServiceFactory
      {
        UserService = new Registration<IUserService>(resolver => userService),
        ClientStore = new Registration<IClientStore>(resolver => clientStore),
        ScopeStore = new Registration<IScopeStore>(resolver => kernel.Get<IScopeStore>()),
        CorsPolicyService = new Registration<ICorsPolicyService>(resolver => corsService),
        ViewService = new Registration<IViewService, MvcViewService<AccountController>>(),
        TokenSigningService = new Registration<ITokenSigningService, MyTokenSigningService>(),
        TokenHandleStore = new Registration<ITokenHandleStore>(resolver => kernel.Get<EntityFrameworkTokenHandleStore>())
      };

      // These registrations are also needed since these are dealt with using non-standard construction
      factory.Register(new Registration<HttpContext>(resolver => HttpContext.Current));
      factory.Register(new Registration<HttpContextBase>(resolver => new HttpContextWrapper(resolver.Resolve<HttpContext>())));
      factory.Register(new Registration<HttpRequestBase>(resolver => resolver.Resolve<HttpContextBase>().Request));
      factory.Register(new Registration<HttpResponseBase>(resolver => resolver.Resolve<HttpContextBase>().Response));
      factory.Register(new Registration<HttpServerUtilityBase>(resolver => resolver.Resolve<HttpContextBase>().Server));
      factory.Register(new Registration<HttpSessionStateBase>(resolver => resolver.Resolve<HttpContextBase>().Session));


      var options = new IdentityServerOptions
      {
        SiteName = Strings.ThisServiceName,
        EnableWelcomePage = false,
        SigningCertificate = signingCert,
        Factory = factory,
        RequireSsl = false,
        CspOptions = new CspOptions
        {
          ImgSrc = "'self' data:"
        },
        AuthenticationOptions = new AuthenticationOptions
        {
          // Try to prevent "request too long" errors when authenticating with Google, etc
          // https://github.com/IdentityServer/IdentityServer3/issues/1124
          SignInMessageThreshold = 1,
          IdentityProviders = ConfigureIdentityProviders,
          LoginPageLinks = new LoginPageLink[] {
                            new LoginPageLink{
                                Text = "Register",
                                //Href = "~/localregistration"
                                Href = "localregistration"
                            }
                        },
          EnablePostSignOutAutoRedirect = true
        },

        EventsOptions = new EventsOptions
        {
          RaiseSuccessEvents = true,
          RaiseErrorEvents = true,
          RaiseFailureEvents = true,
          RaiseInformationEvents = true
        }
      };

      app.UseIdentityServer(options);

      // This must come after .UseIdentityServer so APIs can get identity values off the OWIN context
      WebSetup.SetupWebApi(app, kernel);
    }

    public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
    {
      var googleId = ConfigurationManager.AppSettings["google:clientId"];
      var googleSecret = ConfigurationManager.AppSettings["google:clientSecret"];

      if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
      {
        var google = new GoogleOAuth2AuthenticationOptions
        {
          AuthenticationType = "Google",
          Caption = "Google",
          SignInAsAuthenticationType = signInAsType,
          ClientId = googleId,
          ClientSecret = googleSecret
        };
        app.UseGoogleAuthentication(google);
      }

      var facebookId = ConfigurationManager.AppSettings["facebook:appId"];
      var facebookSecret = ConfigurationManager.AppSettings["facebook:appSecret"];
      if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
      {
        var fb = new FacebookAuthenticationOptions
        {
          AuthenticationType = "Facebook",
          Caption = "Facebook",
          SignInAsAuthenticationType = signInAsType,
          AppId = facebookId,
          AppSecret = facebookSecret
        };
        app.UseFacebookAuthentication(fb);
      }

      foreach (var openIdType in (ConfigurationManager.AppSettings["openId:providers"] ?? string.Empty).Split(','))
      {
        var type = openIdType.Trim();
        var prefix = "openid:" + type + ":";

        var openId = ConfigurationManager.AppSettings[prefix + "clientId"];
        var openSecret = ConfigurationManager.AppSettings[prefix + "clientSecret"];
        var caption = ConfigurationManager.AppSettings[prefix + "caption"];
        var authority = ConfigurationManager.AppSettings[prefix + "authority"];

        if (!string.IsNullOrWhiteSpace(openId)
          && !string.IsNullOrWhiteSpace(openSecret)
          && !string.IsNullOrWhiteSpace(caption)
          && !string.IsNullOrWhiteSpace(authority))
        {
          app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
          {
            AuthenticationType = type,
            ClientId = openId,
            ClientSecret = openSecret,
            Authority = authority,
            Caption = caption,
            SignInAsAuthenticationType = signInAsType
          });
        }
      }
    }
  }

  internal class MyTokenSigningService : DefaultTokenSigningService
  {
    public MyTokenSigningService(ISigningKeyService keyService) : base(keyService)
    {
    }

    protected override string CreatePayload(Token token)
    {
      // Default implementation doesn't allow for much clock skew on the NotBefore claim.
      // It's optional, so let's remove it.
      var result = base.CreatePayload(token);
      var payload = JsonConvert.DeserializeObject<JObject>(result);
      payload.Remove(Constants.ClaimTypes.NotBefore);
      result = JsonConvert.SerializeObject(payload);
      return result;
    }
  }
}