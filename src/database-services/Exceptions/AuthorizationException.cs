using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kcsara.Database
{
  public class AuthorizationException : ApplicationException
  {
    public AuthorizationException(string message = "Access Denied") : base(message)
    {
    }
  }
}
