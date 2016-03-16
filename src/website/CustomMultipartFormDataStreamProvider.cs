﻿// From https://github.com/stewartm83/angular-fileupload-sample
using System.Net.Http;
using System.Net.Http.Headers;

namespace Kcsara.Database.Web
{
  public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
  {
    public CustomMultipartFormDataStreamProvider(string rootPath) : base(rootPath)
    {
    }

    public CustomMultipartFormDataStreamProvider(string rootPath, int bufferSize) : base(rootPath, bufferSize)
    {
    }

    public override string GetLocalFileName(HttpContentHeaders headers)
    {
      //Make the file name URL safe and then use it & is the only disallowed url character allowed in a windows filename
      var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName)
        ? headers.ContentDisposition.FileName
        : "NoName";
      return name.Trim('"').Replace("&", "and");
    }
  }
}