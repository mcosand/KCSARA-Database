
namespace Kcsara.Database.Web
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web.Mvc;

    public class JsonDataContractResult : DataActionResult
    {
        private string jsonString = null;

        public JsonDataContractResult(Object data) : base(data)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Write(this.GetJsonString());
        }

        protected override void OnSet()
        {
            base.OnSet();
            jsonString = null;
        }

        public string GetJsonString()
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                var serializer = new DataContractJsonSerializer(this.Data.GetType());
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, this.Data);
                    jsonString = Encoding.Default.GetString(ms.ToArray());
                }
            }
            return jsonString;

        }
    }
}
