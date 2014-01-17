/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kcsara.Database.Web.Models
{
    public class FileUploadResult
    {
        public string url { get; set; }
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
    }
}
