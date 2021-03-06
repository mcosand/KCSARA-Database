﻿using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kcsara.Database.Web
{
  public class RouteConfig
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      // Don't interfere with ASP.NET chart control in httpHandlers
      routes.IgnoreRoute("{*chartimg}", new { chartimg = @".*/ChartImg\.axd(/.*)?" });

      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapMvcAttributeRoutes();

      routes.MapRoute(
        name: "Register",
        url: "auth/{action}",
        defaults: new { controller = "Register" },
        namespaces: new[] { "Sar.Auth.Controllers" }
        );


      routes.MapRoute(
        name: "ErrorPage",
        url: "Error/{message}",
        defaults: new { controller = "Home", action = "Error", message = UrlParameter.Optional }
        );

      routes.MapRoute(
          name: "Default",
          url: "{controller}/{action}/{id}",
          defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
      );
    }
  }
}
