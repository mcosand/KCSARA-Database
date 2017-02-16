using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using IdentityServer3.AccessTokenValidation;
using log4net;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Sar.Web;

using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Kcsara.Database.Api
{
  public static class AppBuilderExtensions
  {
    public static IAppBuilder UseDatabaseApi(this IAppBuilder app, IKernel kernel, X509Certificate2 signingCert)
    {
      NameValueCollection configStrings = ConfigurationManager.AppSettings;

      JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

      var tokenAuthOptions = new IdentityServerBearerTokenAuthenticationOptions
      {
        Authority = configStrings["auth:authority"],
        IssuerName = configStrings["auth:authority"],
        SigningCertificate = signingCert,
        TokenProvider = new QueryStringOAuthBearerProvider("access_token")
      };

      var config = new HttpConfiguration();
      // Getting Ninject OwinHost and Ninject WebHost to live together isn't really supported.
      // I think this is the guts of .UseNinjectMiddleware but there may be bugs here.
      config.DependencyResolver = new OwinNinjectDependencyResolver(kernel);

      config.MapHttpAttributeRoutes(new InheritanceDirectRouteProvider());

      config.Filters.Add(new AuthorizeAttribute());
      config.Services.Replace(
         typeof(IExceptionHandler),
         new ExceptionHandler(LogManager.GetLogger("Kcsara.Database.Api")));


      config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
      config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new ItemPermissionConverter());
      config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      config.SuppressDefaultHostAuthentication();
      config.Filters.Add(new HostAuthenticationFilter(tokenAuthOptions.AuthenticationType));

      config.EnableCors();

      config.Services.Replace(typeof(IHttpControllerTypeResolver), new NamespaceControllerTypeResolver("Kcsara.Database.Api.Controllers"));

      var userService = kernel.Get<Sar.Database.Services.IUsersService>();
      app.UseIdentityServerBearerTokenAuthentication(tokenAuthOptions);
      app.Use(new Func<AppFunc, AppFunc>(next => (async env =>
      {
        var identity = new Microsoft.Owin.OwinContext(env)?.Authentication?.User?.Identity as ClaimsIdentity;
        var subClaim = identity?.FindFirst("sub")?.Value;
        Guid sub;
        if (!string.IsNullOrWhiteSpace(subClaim) && Guid.TryParse(subClaim, out sub))
        {
          var user = await userService.GetUser(sub);
          identity.AddClaim(new Claim("name", user.Name));
        }

        await next.Invoke(env);
      })));
      app.UseWebApi(config);

      return app;
    }

    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
      readonly string _name;

      public QueryStringOAuthBearerProvider(string name)
      {
        _name = name;
      }

      public override Task RequestToken(OAuthRequestTokenContext context)
      {
        var queryValue = context.Request.Query.Get(_name);
        var headerValue = context.Request.Headers.Get("Authorization");

        if (!string.IsNullOrWhiteSpace(headerValue) && headerValue.StartsWith("Bearer "))
        {
          context.Token = headerValue.Substring(7);
        } else if (!string.IsNullOrEmpty(queryValue))
        {
          context.Token = queryValue;
        }

        return Task.FromResult<object>(null);
      }
    }
  }

  class InheritanceDirectRouteProvider : DefaultDirectRouteProvider
  {
    protected override IReadOnlyList<IDirectRouteFactory> GetControllerRouteFactories(HttpControllerDescriptor controllerDescriptor)
    {
      // Inherit route attributes decorated on base class controller
      // GOTCHA: RoutePrefixAttribute doesn't show up here, even though we were expecting it to.
      //  Am keeping this here anyways, but am implementing an ugly fix by overriding GetRoutePrefix
      return controllerDescriptor.GetCustomAttributes<IDirectRouteFactory>(true);
    }

    protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
    {
      // Inherit route attributes decorated on base class controller's actions
      return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(true);
    }
  }
}
