/*
 * Copyright Matthew Cosand
 */
using System.Web.Mvc;
using System.Web.Routing;

namespace Sar.Auth
{
  public class RouteConfig
  {
    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
      routes.MapMvcAttributeRoutes();
    }
  }
}
