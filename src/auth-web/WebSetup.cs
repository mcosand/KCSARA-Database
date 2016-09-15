namespace Sar.Auth
{
  using System;
  using System.Collections.Specialized;
  using System.IdentityModel.Tokens;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using System.Web;
  using System.Web.Http;
  using System.Web.Http.ExceptionHandling;
  using IdentityModel.Client;
  using Microsoft.IdentityModel.Protocols;
  using Microsoft.Owin.Security;
  using Microsoft.Owin.Security.OpenIdConnect;
  using Newtonsoft.Json.Serialization;
  using Ninject;
  using Ninject.Web.Common;
  using Ninject.Web.WebApi;
  using Owin;
  using Serilog;

  public static class WebSetup
  {
    public static IKernel StartMvcAndWebApiWithNinject(IAppBuilder app, NameValueCollection configStrings, Action<IKernel> registerServices)
    {
      var kernel = SetupDependencyInjection(registerServices, configStrings);
      SetupWebApi(app, kernel);
      return kernel;
    }

    public static IKernel SetupDependencyInjection(Action<IKernel> registerServices, NameValueCollection configStrings)
    {
      var kernel = new StandardKernel();
      var bootstrapper = new Bootstrapper();
      bootstrapper.Initialize(() => kernel);
      kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

      var logConfig = new LoggerConfiguration()
          .MinimumLevel.Information()
          .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-{Date}.txt");
      Log.Logger = logConfig.CreateLogger();

      kernel.Bind<ILogger>().ToConstant(Log.Logger);
      if (registerServices != null)
      {
        registerServices(kernel);
      }

      return kernel;
    }

    public static void SetupOidcCookieAuth(IAppBuilder app, NameValueCollection configStrings, string[] addScopes = null)
    {
      app.Use<NonceCleanupOpenIdConnectAuthenticationMiddleware>(app, new OpenIdConnectAuthenticationOptions
      {
        Authority = configStrings["auth:authority"].Trim('/') + "/",
        ClientId = configStrings["auth:client_id"],
        RedirectUri = configStrings["auth:redirect"].Trim('/') + "/",
        ResponseType = "code id_token token",
        Scope = "openid email profile kcsara-profile" + (addScopes.Length > 0 ? " " + string.Join(" ", addScopes) : string.Empty),
        TokenValidationParameters = new TokenValidationParameters
        {
          NameClaimType = "name"
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
    }

    public static void SetupWebApi(IAppBuilder app, IKernel kernel)
    {
      var config = new HttpConfiguration();
      config.MapHttpAttributeRoutes();
      config.Services.Replace(typeof(IExceptionHandler), new ApiUserExceptionHandler());
      config.Services.Add(typeof(IExceptionLogger), kernel.Get<ApiExceptionLogger>());

      var formatter = config.Formatters.JsonFormatter;
      formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      config.DependencyResolver = new NinjectDependencyResolver(kernel);

      config.EnableCors();

      app.UseWebApi(config);
    }
  }
}
