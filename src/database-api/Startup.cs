using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Sar.Services;
using Sar.Services.Auth;

[assembly: OwinStartup(typeof(Kcsara.Database.Api.Startup))]

namespace Kcsara.Database.Api
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      NameValueCollection configStrings = ConfigurationManager.AppSettings;

      FileInfo logConfigPath = new FileInfo(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "log4net.config"));
      log4net.Config.XmlConfigurator.ConfigureAndWatch(logConfigPath);
      log4net.LogManager.GetLogger(typeof(Startup)).Info("Starting ...");

      IKernel kernel = new StandardKernel();
      kernel.Bind<IHost>().ToMethod(context => new OwinHost());
      kernel.Bind<IAuthenticatedHost>().ToMethod(context => new OwinHost());
      kernel.Load(new Services.DIModule());
      JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

      var tokenAuthOptions = new IdentityServerBearerTokenAuthenticationOptions
      {
        Authority = configStrings["auth:authority"]
        //RequiredScopes = new[] { "db" }
        // client credentials for the introspection endpoint
        //ClientId = configStrings["auth:clientId"],
        //ClientSecret = configStrings["auth:secret"]
      };

      var config = new HttpConfiguration();
      config.MapHttpAttributeRoutes();

      config.Filters.Add(new ExceptionFilter());
      config.Filters.Add(new AuthorizeAttribute());

      config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
      config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      config.SuppressDefaultHostAuthentication();
      config.Filters.Add(new HostAuthenticationFilter(tokenAuthOptions.AuthenticationType));

      config.EnableCors();

      app.UseIdentityServerBearerTokenAuthentication(tokenAuthOptions);
      app.UseNinjectMiddleware(() => kernel);
      app.UseNinjectWebApi(config);
    }
  }
}
