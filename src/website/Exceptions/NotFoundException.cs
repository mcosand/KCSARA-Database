/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System.Net;

  public class NotFoundException : StatusCodeException
  {
    public NotFoundException()
      : base(HttpStatusCode.NotFound, "Object not found")
    {
    }
  }
}
