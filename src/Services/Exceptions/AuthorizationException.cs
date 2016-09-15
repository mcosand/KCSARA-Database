using System;

namespace Kcsara.Database
{
  public class AuthorizationException : ApplicationException
  {
    public AuthorizationException(string message = "Access Denied") : base(message)
    {
    }
  }
}
