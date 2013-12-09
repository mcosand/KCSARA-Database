
namespace Kcsara.Database.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Web.Mvc;

    public class StreamResult : ViewResult
    {
        private Stream _stream = new MemoryStream();
        public Stream Stream
        {
            get
            {
                return _stream;
            }

            set
            {
                try
                {
                    _stream.Dispose();
                }
                finally { }
                _stream = value;
            }
        }

        public string ContentType { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;
            const int size = 4096;
            byte[] bytes = new byte[size];
            int numBytes;
            while ((numBytes = _stream.Read(bytes, 0, size)) > 0)
                context.HttpContext.Response.OutputStream.Write(bytes, 0, numBytes);
            _stream.Dispose();
        }
    }

}
