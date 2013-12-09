
namespace Kcsara.Database.Web
{
    using System;
    using System.Web.Management;
    using System.Web.Mvc;

    public class HandleErrorHmAttribute : HandleErrorAttribute {
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            new WebRequestErrorEventMvc("An unhandled exception has occurred.", this, 103005, context.Exception).Raise();
        }
    }

    public class WebRequestErrorEventMvc : WebRequestErrorEvent
    {
        public WebRequestErrorEventMvc(string message, object eventSource, int eventCode, Exception exception) : base(message, eventSource, eventCode, exception) { }
        public WebRequestErrorEventMvc(string message, object eventSource, int eventCode, int eventDetailCode, Exception exception) : base(message, eventSource, eventCode, eventDetailCode, exception) { }
    }
}