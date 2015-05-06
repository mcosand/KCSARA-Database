/*
 * Copyright 2015 Matthew Cosand
 */

[assembly: Microsoft.Owin.OwinStartup(typeof(Kcsara.Database.Web.Startup))]

namespace Kcsara.Database.Web
{
  using System;
  using Microsoft.AspNet.SignalR;
  using Newtonsoft.Json;
  using Owin;

  public class Startup
  {
    private static readonly Lazy<JsonSerializer> JsonSerializerFactory = new Lazy<JsonSerializer>(GetJsonSerializer);
    private static JsonSerializer GetJsonSerializer()
    {
      return new JsonSerializer
      {
        ContractResolver = new FilteredCamelCasePropertyNamesContractResolver("Kcsar")
      };
    }

    public void Configuration(IAppBuilder app)
    {
      app.MapSignalR();
      GlobalHost.DependencyResolver.Register(
        typeof(JsonSerializer),
        () => JsonSerializerFactory.Value);
    }
  }
}
