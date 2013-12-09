
namespace Kcsara.Database.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    public class DataActionResult : ActionResult
    {
        public DataActionResult(object data)
        {
            this.Data = data;
        }

        private object data;
        public object Data 
        {
            get
            {
                return data;
            }
            
            protected set
            {
                data = value;
                OnSet();
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/plain";
            context.HttpContext.Response.Write(this.data.ToString());
        }

        protected virtual void OnSet()
        {
        }
    }
}
