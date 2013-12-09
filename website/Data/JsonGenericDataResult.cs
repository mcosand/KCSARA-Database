
namespace Kcsara.Database.Web
{
    using System;
    using System.Web.Mvc;

    public class JsonGenericDataResult : DataActionResult
    {
        public JsonGenericDataResult(Object data) : base(data)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            JsonResult innerAction = new JsonResult { Data = this.Data };
            innerAction.ExecuteResult(context);
        }

        protected override void OnSet()
        {
            base.OnSet();
        }
    }
}
