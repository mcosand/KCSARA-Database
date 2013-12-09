namespace Kcsara.Database.Web.Controllers
{
    using System;
    using System.Web.Mvc;

    public interface IEventController
    {
        ActionResult Roster(Guid id);
    }
}
