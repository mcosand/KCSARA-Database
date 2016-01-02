/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
  using System.Collections.Generic;

  internal class ModelErrorsException : StatusCodeException
  {
    public Dictionary<string, string> Errors { get; private set; }

    public ModelErrorsException(Dictionary<string, string> errors)
      : base(System.Net.HttpStatusCode.BadRequest, "Invalid request")
    {
      Errors = errors;
    }
  }
}