/*
 * Copyright 2011-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web.Mvc;
    using System.Xml;

    public class XmlDataContractResult : DataActionResult
    {
        private string xmlString = null;

        public XmlDataContractResult(Object data) : base(data)
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
                //var serializer = new DataContractSerializer(this.Data.GetType());
                //using (var ms = new MemoryStream())
                //{
                //    serializer.WriteObject(ms, this.Data);
                //    xmlString = Encoding.Default.GetString(ms.ToArray());
                //}

                DataContractSerializer dataContractSerializer = new DataContractSerializer(this.Data.GetType());

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();

                xmlWriterSettings.Indent = true;

                xmlWriterSettings.CheckCharacters = false;

                xmlWriterSettings.NewLineHandling = NewLineHandling.Entitize;

                using (MemoryStream ms = new MemoryStream())
                {

                    XmlWriter xmlWriter = XmlWriter.Create(ms, xmlWriterSettings);

                    dataContractSerializer.WriteObject(xmlWriter, this.Data);

                    xmlWriter.Close();

                    ms.Position = 0;
                    xmlString = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return xmlString.Replace(" xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"", "").Replace("d3p1:","");

        }
    }
}
