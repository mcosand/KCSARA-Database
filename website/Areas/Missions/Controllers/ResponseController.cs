using System;
using System.Web.Mvc;
using Kcsara.Database.Web.api.Models;
using Kcsara.Database.Web.Areas.Missions.api;
using Kcsara.Database.Web.Controllers;
using A = Kcsara.Database.Web.api;

namespace Kcsara.Database.Web.Areas.Missions.Controllers
{
  public class ResponseController : BaseController
  {
    protected ControllerArgs args;
    public ResponseController(ControllerArgs args) : base (args)
    {
      this.args = args;
    }

    [Authorize]
    public ActionResult Index()
    {
      var responseApi = new ResponseApiController(this.args);
      ViewBag.Data = responseApi.GetCurrentStatus();
      AddMyUnits();
      
      return View();
    }

    [Authorize]
    public ActionResult Create()
    {
      ViewBag.IsMissionEditor = Permissions.IsInRole("cdb.missioneditors");
      return View();
    }

    [Authorize]
    public ActionResult Info(Guid id)
    {
      var responseApi = new ResponseApiController(this.args);
      ViewBag.Data = responseApi.GetMissionInfo(id);
      ViewBag.MissionId = id;
      AddMyUnits();
      
      return View();
    }

    [Authorize]
    public ActionResult Dashboard()
    {
      AddMyUnits();

      return View();
    }

    private void AddMyUnits()
    {
      var members = new A.MembersController(this.args);
      Guid? userId = this.Permissions.UserId;
      ViewBag.MyUnits = userId.HasValue ? members.GetActiveUnits(this.Permissions.UserId) : new NameIdPair[0];
    }
  }
}
