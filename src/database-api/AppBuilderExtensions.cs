using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using IdentityServer3.AccessTokenValidation;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Sar.Services;
using Sar.Services.Auth;

namespace Kcsara.Database.Api
{
  public static class AppBuilderExtensions
  {
    public static IAppBuilder UseDatabaseApi(this IAppBuilder app, IKernel kernel)
    {
      NameValueCollection configStrings = ConfigurationManager.AppSettings;

      JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

      var tokenAuthOptions = new IdentityServerBearerTokenAuthenticationOptions
      {
        Authority = configStrings["auth:authority"]
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
  }
}
