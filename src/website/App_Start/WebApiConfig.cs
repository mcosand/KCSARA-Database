/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web.api
{
  using System.Web.Http;
  using Newtonsoft.Json.Converters;
  using Newtonsoft.Json.Serialization;
  using Ninject;

  /// <summary>Configure API settings for the basic application</summary>
  public static class WebApiConfig
  {
    /// <summary>Configures API settings for the basic application.</summary>
    /// <param name="config">Configuration of the application.</param>
    /// <param name="kernel">Composition root.</param>
    public static void Register(HttpConfiguration config, IKernel kernel)
    {
      config.DependencyResolver = new NinjectResolver(kernel);

      config.Routes.MapHttpRoute(
          "DefaultApi",
          "api/{controller}/{action}/{id}",
          new { action = RouteParameter.Optional, id = RouteParameter.Optional }
      );

      config.Filters.Add(new ExceptionFilter());
      config.Filters.Add(new AuthorizeAttribute());
      
      var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
      jsonSettings.Converters.Add(new StringEnumConverter());
      jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

    }
  }
}
