/*
 * Copyright 2010-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
    using System.IO;
    using System.Text;
    using System.Web.Mvc;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlDataResult : DataActionResult
    {
        private string xmlString = null;


        public XmlDataResult(object data) : base(data)
        {
        }
        
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/xml";
            context.HttpContext.Response.Write(this.GetXmlString());
        }

        protected override void OnSet()
        {
            base.OnSet();
            xmlString = null;
        }

        public string GetXmlString()
        {
            if (string.IsNullOrEmpty(xmlString))
            {
                XmlSerializer ser = new XmlSerializer(this.Data.GetType());
                using (MemoryStream s = new MemoryStream())
                {
                    XmlTextWriter write = new XmlTextWriter(s, System.Text.Encoding.UTF8);
                    write.Formatting = Formatting.Indented;
                    write.Indentation = 2;
                    ser.Serialize(write, this.Data);
                    write.Close();
                    s.Close();
                    xmlString = Encoding.UTF8.GetString(s.GetBuffer()).Trim((char)0);
                }
            }
            return xmlString;

        }
    }
}
