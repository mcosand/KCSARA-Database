using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Sar.Web;

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

      config.MapHttpAttributeRoutes();

      config.Filters.Add(new ExceptionFilter());
      config.Filters.Add(new AuthorizeAttribute());

      config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
      config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      config.SuppressDefaultHostAuthentication();
      config.Filters.Add(new HostAuthenticationFilter(tokenAuthOptions.AuthenticationType));

      config.EnableCors();

      config.Services.Replace(typeof(IHttpControllerTypeResolver), new NamespaceControllerTypeResolver("Kcsara.Database.Api.Controllers"));

      app.UseIdentityServerBearerTokenAuthentication(tokenAuthOptions);
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
}
