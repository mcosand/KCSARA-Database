/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System.Net;

  public class AccessDeniedException : StatusCodeException
  {
    public AccessDeniedException()
      : base(HttpStatusCode.Forbidden, "Access Denied")
    {
    }
  }
}
