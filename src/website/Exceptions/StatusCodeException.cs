/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System;
  using System.Net;

  public class StatusCodeException : Exception
  {
    public HttpStatusCode StatusCode { get; private set; }

    public StatusCodeException(HttpStatusCode statusCode, string message) : base(message)
    {
      StatusCode = statusCode;
    }
  }
}
