/*
 * Copyright 2012-2015 Matthew Cosand
 */
using System.Web.Http;
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

      routes.MapRoute(
        name: "ErrorPage",
        url: "Error/{message}",
        defaults: new { controller = "Home", action = "Error", message = UrlParameter.Optional }
        );

      routes.MapRoute(
        name: "NewMissionList",
        url: "Missions/List/{id}",
        defaults: new { controller = "NewMissions", action = "List", id = UrlParameter.Optional });
      routes.MapRoute(
        name: "NewMissionIndex",
        url: "Missions/",
        defaults: new { controller = "NewMissions", action = "Index" });
      routes.MapRoute(
        name: "NewTrainingList",
        url: "Training/List/{id}",
        defaults: new { controller = "NewTraining", action = "List", id = UrlParameter.Optional });
      routes.MapRoute(
        name: "NewTrainingIndex",
        url: "Training/",
        defaults: new { controller = "NewTraining", action = "Index" });

      routes.MapRoute(
          name: "Default",
          url: "{controller}/{action}/{id}",
          defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
      );
    }
  }
}
