using System.Web.Mvc;

namespace Kcsara.Database.Web.Areas.Missions
{
  public class MissionsAreaRegistration : AreaRegistration
  {
    public const string RouteName = "MissionResponse_default";

    public override string AreaName
    {
      get
      {
        return "Missions";
      }
    }

    public override void RegisterArea(AreaRegistrationContext context)
    {
      context.MapRoute(
          RouteName,
          "Missions/Response/{action}/{id}",
          new { controller = "Response", action = "Index", id = UrlParameter.Optional }
      );      
    }
  }
}
